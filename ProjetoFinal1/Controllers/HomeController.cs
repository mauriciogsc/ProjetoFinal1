
using FourSquare.SharpSquare.Core;
using FourSquare.SharpSquare.Entities;
using System;
using Banco.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Data.Entity;

namespace ProjetoFinal1.Controllers
{
    public class HomeController : Controller
    {
        string clientId = "V0LRK1PVS30TMTHFQNUFWCZC5C4ZBE4J02O5EXF4O421FIAI";
        string clientSecret = "REP3R12EZYWKOA0PPTMBSFSU5SRGS1UBV2VZVVOW2VIWGZB0";
        string redirectUri = "REDIRECT_URI";
        BancoContext db = new BancoContext();
        public ActionResult venues()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            BancoContext db = new BancoContext();
            int lastid = 0;
            Dictionary<string, string> parametros = new Dictionary<string, string>();

            List<Banco.Models.Venue> lisVenue = db.Venues.Where(w => w.updated.Year < 1950).ToList();

            foreach (Banco.Models.Venue ven in lisVenue)
            {
                try
                {
                    FourSquare.SharpSquare.Entities.Venue v = new FourSquare.SharpSquare.Entities.Venue();
                    lastid = ven.Id;
                    v = sharpSquare.GetVenue(ven.SquareId);
                    ven.checkincount = (int)v.stats.checkinsCount;
                    ven.tipcount = (int)v.stats.tipCount;
                    ven.rate = v.rating;
                    if (v.price != null)
                        ven.tier = v.price.tier;
                    ven.likes = (int)v.likes.count;
                    ven.updated = DateTime.Now;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    if (e.Message == "O servidor remoto retornou um erro: (403) Proibido.")
                    {
                        ViewBag.Message = e.Message;
                        break;
                    }
                }
            }
            return View();
        }

        public async System.Threading.Tasks.Task<bool> GetAlchemyRate()
        {
            var client = new HttpClient();
            var count = 0;
            // Create the HttpContent for the form to be posted.
            var tips = db.Tips.Where(w => w.AlchemyPredict == 0 && w.status != 0);
            foreach (Banco.Models.Tip tip in tips)
            {
                var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("text", tip.Description),
                    });

                // Get the response.
                HttpResponseMessage response = await client.PostAsync(
                    "https://gateway-a.watsonplatform.net/calls/text/TextGetTextSentiment?apikey=ea0a5ea9d3f099773804971a60bccdfd75768e79&outputMode=json",
                    requestContent);

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    // Write the output.
                    var resp = await reader.ReadToEndAsync();
                    var jobj = JObject.Parse(resp);
                    if (jobj["status"].ToString() == "OK" && jobj["docSentiment"]["type"] != null)
                    {

                        string Status = jobj["docSentiment"]["type"].ToString();
                        if (Status == "negative")
                            tip.AlchemyPredict = 3;
                        if (Status == "positive")
                            tip.AlchemyPredict = 1;
                        if (Status == "neutral")
                            tip.AlchemyPredict = 2;
                        if (jobj["docSentiment"]["mixed"] != null)
                            tip.AlchemyMixed = int.Parse(jobj["docSentiment"]["mixed"].ToString());
                        if (jobj["docSentiment"]["score"] != null)
                        {
                            string score = jobj["docSentiment"]["score"].ToString();
                            score = score.Replace('.', ',');
                            tip.AlchemyScore = float.Parse(score);
                        }
                        db.Entry(tip).State = EntityState.Modified;

                    }
                    else
                    {
                        if (jobj["statusInfo"].ToString() != "unsupported-text-language")
                        {
                            break;
                        }
                    }
                }
            }

            db.SaveChanges();
            return true;
        }
        private double CalculateStdDev(IEnumerable<int> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        public ActionResult Satisfacao()
        {
            List<Banco.Models.User> users = db.Users.ToList();

            //números obtidos pelo sql server para essa amostra
            double desviopadrao = 1.67239257986971;
            double mediaTotal = 7.92828871470173;
            foreach (Banco.Models.User u in users)
            {
                List<Banco.Models.Tip> tipsUser = db.Tips.Where(w => w.UserId == u.Id && w.WekaPredictFinal != 0).ToList();
                int somaComentarios = 0;
                foreach (Banco.Models.Tip t in tipsUser)
                {
                    if (t.WekaPredictFinal == 1)
                    {
                        somaComentarios += 10;
                    }
                    else if (t.WekaPredictFinal == 2)
                    {
                        somaComentarios += 5;
                    }
                }
                if (tipsUser.Count == 0)
                    u.mediaComentarios = 0;
                else
                    u.mediaComentarios = Convert.ToDouble(somaComentarios) / Convert.ToDouble(tipsUser.Count);
                if (tipsUser.Count < 5)
                    u.weight = 1;
                else if (u.mediaComentarios == mediaTotal)
                    u.weight = 3;
                else
                    u.weight = (3 * (float)desviopadrao) / Math.Abs((float)u.mediaComentarios - (float)mediaTotal);
                if (u.weight > 3)
                    u.weight = 3;
                if (u.weight < 1)
                    u.weight = 1;
            }
            List<Banco.Models.Venue> venues = db.Venues.ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                List<Banco.Models.Tip> tipsVenue = db.Tips.Where(w => w.VenueId == v.Id).ToList();
                float somaPesos = 0;
                float somaTotal = 0;
                foreach (Banco.Models.Tip t in tipsVenue)
                {
                    if (t.WekaPredictFinal == 1)
                    {
                        somaTotal += 10 * t.User.weight;
                    }
                    else if (t.WekaPredictFinal == 2)
                    {
                        somaTotal += 5 * t.User.weight;
                    }
                    somaPesos += t.User.weight;
                }
                if (somaPesos == 0)
                    v.rateWeka = 0;
                else
                    v.rateWeka = (double)somaTotal / (double)somaPesos;
            }
            db.SaveChanges();
            return View();
            //Banco.Models.Venue v = db.Venues.Find(131);
            //List<Banco.Models.Tip> tips = db.Tips.Where(w => w.VenueId == 131 && w.WekaPredict!=0).ToList();
            //float media = 0.0f;
            //float total = 0.0f;
            //foreach(Banco.Models.Tip t in tips)
            //{
            //    if(t.WekaPredict == 1)
            //    {
            //        total += 10.0f;
            //    }
            //    if (t.WekaPredict == 2)
            //    {
            //        total += 5.0f;
            //    }

            //}
            //media = total / tips.Count;
            //ViewBag.media = media;
            //ViewBag.tips = tips.Take(15);
            //return View();
        }

        public void WriteARFF()
        {

            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\Avell B155 MAX\\Documents\\facul\\projetofinal\\lasttestDuasClassesMedianaComp.arff");
            file.WriteLine("% 1. Title: Sentiment Analysis\n");
            file.WriteLine("@RELATION tips\n");
            file.WriteLine("@ATTRIBUTE sent1        NUMERIC");
            file.WriteLine("@ATTRIBUTE sent2        NUMERIC");
            file.WriteLine("@ATTRIBUTE sent3        NUMERIC");
            file.WriteLine("@ATTRIBUTE sent4        NUMERIC");
            file.WriteLine("@ATTRIBUTE sent5        NUMERIC");
            file.WriteLine("@ATTRIBUTE mean1        NUMERIC");
            file.WriteLine("@ATTRIBUTE mean2        NUMERIC");
            file.WriteLine("@ATTRIBUTE mean3        NUMERIC");
            file.WriteLine("@ATTRIBUTE mean4        NUMERIC");
            file.WriteLine("@ATTRIBUTE mean5        NUMERIC");
            file.WriteLine("@ATTRIBUTE stdv1        NUMERIC");
            file.WriteLine("@ATTRIBUTE stdv2        NUMERIC");
            file.WriteLine("@ATTRIBUTE stdv3        NUMERIC");
            file.WriteLine("@ATTRIBUTE stdv4        NUMERIC");
            file.WriteLine("@ATTRIBUTE stdv5        NUMERIC");
            file.WriteLine("@ATTRIBUTE class        {0,1}\n");
            file.WriteLine("@DATA");
            var tips = db.Tips.Where(w => w.Venue.rate != 0 && w.WekaPredictFinal != 0).GroupBy(g => g.VenueId);
           
            foreach (var g in tips)
            {
                string line = "";
                List<int> sents = new List<int>();
                List<double> means = new List<double>();
                List<float> stdvs = new List<float>();
                string classe = "0";
                if (g.Count() > 4)
                {
                    Banco.Models.Venue v = db.Venues.Find(g.Key);
                    //if (v.rate < 5)
                    //    classe = "0";
                    //else if (v.rate >= 5 && v.rate <= 6)
                    //    classe = "1";
                    //else if (v.rate > 6 && v.rate <= 8)
                    //    classe = "2";
                    //else
                    //    classe = "3";
                    classe = v.rate > 6.9 ? "1" : "0";
                    foreach (var t in g.Take(5))
                    {
                        sents.Add(t.WekaPredictFinal);
                        if (t.User.pesoInterno != 1)
                        {
                            means.Add(t.User.mediaComentarios);
                            stdvs.Add(t.User.pesoInterno);
                        }
                    }
                    if (means.Count != 0)
                    {
                        if (means.Count != 5)
                        {
                            int cont = means.Count;
                            for (int i = 0; i < 5 - cont; i++)
                            {
                                means.Add(means.Sum() / means.Count);
                                stdvs.Add(stdvs.Sum() / stdvs.Count);
                            }
                        }
                        foreach (int sent in sents)
                        {
                            line += sent.ToString() + ",";
                        }
                        foreach (double mean in means)
                        {
                            line += mean.ToString().Replace(',', '.') + ",";
                        }
                        foreach (float stdv in stdvs)
                        {
                            line += stdv.ToString().Replace(',', '.') + ",";
                        }
                        line += classe;
                        file.WriteLine(line);
                    }
                }
            }

            file.Close();
        }

        public void SatisfacaoInterna()
        {
            List<Banco.Models.User> users = db.Users.ToList();

            foreach (Banco.Models.User u in users)
            {
                List<Banco.Models.Tip> tipsUser = db.Tips.Where(w => w.UserId == u.Id && w.WekaPredictFinal != 0).ToList();
                List<int> listaDesvio = new List<int>();
                foreach (Banco.Models.Tip t in tipsUser)
                {
                    if (t.WekaPredictFinal == 1)
                    {
                        listaDesvio.Add(10);
                    }
                    else if (t.WekaPredictFinal == 2)
                    {
                        listaDesvio.Add(5);
                    }
                    else
                        listaDesvio.Add(0);
                }
                if (tipsUser.Count < 6)
                    u.pesoInterno = 1;
                else
                    u.pesoInterno = (float)CalculateStdDev(listaDesvio);
                //if (u.pesoInterno < 3)
                //    u.pesoInterno = 1;
                //else if (u.pesoInterno >= 3 && u.pesoInterno <= 4)
                //    u.pesoInterno = 2;
                //else if (u.pesoInterno > 4)
                //    u.pesoInterno = 3;
            }
            db.SaveChanges();
            List<Banco.Models.Venue> venues = db.Venues.ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                List<Banco.Models.Tip> tipsVenue = db.Tips.Where(w => w.VenueId == v.Id).ToList();
                float somaPesos = 0;
                float somaTotal = 0;
                foreach (Banco.Models.Tip t in tipsVenue)
                {
                    if (t.WekaPredictFinal == 1)
                    {
                        somaTotal += 10 * t.User.pesoInterno;
                    }
                    else if (t.WekaPredictFinal == 2)
                    {
                        somaTotal += 5 * t.User.pesoInterno;
                    }
                    somaPesos += t.User.pesoInterno;
                }
                if (somaPesos == 0)
                    v.ratePesoInterno = 0;
                else
                    v.ratePesoInterno = somaTotal / (double)somaPesos;
            }
            db.SaveChanges();
        }

        public void satisfacaoSemPonderar()
        {

            List<Banco.Models.Venue> venues = db.Venues.ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                List<Banco.Models.Tip> tipsVenue = db.Tips.Where(w => w.VenueId == v.Id).ToList();
                int count = 0;
                float somaTotal = 0;
                foreach (Banco.Models.Tip t in tipsVenue)
                {
                    if (t.WekaPredictFinal == 1)
                    {
                        somaTotal += 10;
                    }
                    else if (t.WekaPredictFinal == 2)
                    {
                        somaTotal += 5;
                    }
                    count++;
                }
                if (count == 0)
                    v.rateMediaAritmetica = 0;
                else
                    v.rateMediaAritmetica = (double)somaTotal / count;
            }
            db.SaveChanges();
        }

        public ActionResult readPredictFromFile()
        {
            List<char> listaPredict = new List<char>();
            List<string> listId = new List<string>();
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("C:\\Users\\Avell B155 MAX\\Documents\\facul\\projetofinal\\Python notebook\\unclasspredicaofinal.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    sr.ReadLine();
                    sr.ReadLine();
                    sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        listaPredict.Add(line[25]);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("C:\\Users\\Avell B155 MAX\\Documents\\facul\\projetofinal\\Python notebook\\attributes2.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        listId.Add(line);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            var tips = db.Tips.Where(w => w.status == 0).OrderBy(o => o.Id);
            int countId = 0;
            int countProgress = 0;
            foreach (Banco.Models.Tip t in tips)
            {
                countId++;

                if (int.Parse(listId.ElementAt(countProgress)) == countId)
                {
                    t.WekaPredictFinal = int.Parse(listaPredict.ElementAt(countProgress).ToString());
                    countProgress++;
                }
            }
            db.SaveChanges();
            ViewBag.Teste = listId;
            return View();
        }

        public ActionResult setPositivo(int id)
        {
            Banco.Models.Tip tp = db.Tips.Find(id);
            tp.status = 1;
            tp.UpdateDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult setNeutro(int id)
        {
            Banco.Models.Tip tp = db.Tips.Find(id);
            tp.status = 2;
            tp.UpdateDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult setNegativo(int id)
        {
            Banco.Models.Tip tp = db.Tips.Find(id);
            tp.status = 3;
            tp.UpdateDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Index()
        {
            var qry = db.Tips.Where(t => t.status == 0 && t.Venue.tipcount > 9).OrderBy(o => o.Id);


            int count = qry.Count(); // 1st round-trip
            int index = new Random().Next(count);

            Banco.Models.Tip newtip = qry.Skip(index).FirstOrDefault(); // 2nd round-trip
            ViewBag.TipId = newtip.Id;
            ViewBag.Tip = newtip.Description;
            ViewBag.Venue = newtip.Venue.Name;
            return View();

        }

        public ActionResult AdicionarTips()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            List<Banco.Models.Tip> tiplist = new List<Banco.Models.Tip>();
            List<Banco.Models.User> userlist = new List<Banco.Models.User>();
            List<Banco.Models.Venue> venuelist = new List<Banco.Models.Venue>();
            Dictionary<string, string> parametros = new Dictionary<string, string>();
            List<FourSquare.SharpSquare.Entities.Venue> venues = new List<FourSquare.SharpSquare.Entities.Venue>();
            List<FourSquare.SharpSquare.Entities.Tip> tips = new List<FourSquare.SharpSquare.Entities.Tip>();
            parametros.Add("limit", "500"); // tentando pegar ateh 500 venues e tips

            for (double lon = -43.2652; lon < -43.2475; lon += 0.0005)
            {

                parametros.Remove("limit");
                parametros.Add("limit", "50");
                parametros.Add("ll", "-22.8707," + lon.ToString().Replace(',', '.'));
                venues = sharpSquare.SearchVenues(parametros);

                foreach (FourSquare.SharpSquare.Entities.Venue v in venues)
                {
                    Banco.Models.Venue ven;
                    ven = db.Venues.FirstOrDefault(f => f.SquareId == v.id);
                    if (ven == null && venuelist.FirstOrDefault(f => f.SquareId == v.id) == null)
                    {
                        ven = new Banco.Models.Venue();
                        ven.SquareId = v.id;
                        ven.lat = -22.8707;
                        ven.lon = lon;
                        venuelist.Add(ven);
                        db.Venues.Add(ven);
                    }
                    ven.Name = v.name;
                    parametros.Remove("ll");
                    parametros.Remove("limit");
                    parametros.Add("limit", "500");
                    tips = sharpSquare.GetVenueTips(v.id, parametros);
                    foreach (FourSquare.SharpSquare.Entities.Tip t in tips)
                    {
                        Banco.Models.Tip tip;
                        //Verifica de tip já foi adicionada anteriormente no banco
                        tip = db.Tips.FirstOrDefault(f => f.SquareId == t.id);
                        if (tip == null && tiplist.FirstOrDefault(f => f.SquareId == t.id) == null)
                        {
                            //Se é uma tip nova cria uma e adiciona no context
                            tip = new Banco.Models.Tip();
                            tip.SquareId = t.id;
                            tip.Venue = ven;
                            tiplist.Add(tip);
                            db.Tips.Add(tip);

                        }
                        //sendo tip nova ou não atualiza os campos
                        tip.Description = t.text;
                        Banco.Models.User user;
                        user = db.Users.FirstOrDefault(f => f.SquareId == t.user.id);
                        if (user == null && userlist.FirstOrDefault(f => f.SquareId == t.user.id) == null)
                        {
                            user = new Banco.Models.User();
                            user.SquareId = t.user.id;
                            user.Name = t.user.firstName;
                            userlist.Add(user);
                            db.Users.Add(user);
                        }
                        if (user == null && userlist.FirstOrDefault(f => f.SquareId == t.user.id) != null)
                        {
                            user = userlist.FirstOrDefault(f => f.SquareId == t.user.id);
                        }
                        tip.User = user;
                    }
                }
                db.SaveChanges();
            }
            ViewBag.Message = "Venues, tips e users adicionados ao banco com sucesso.";
            return View();
        }

        public ActionResult PreencheCategorias()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            List<Banco.Models.Venue> venues = db.Venues.Include("Categories").Where(w => w.Id > 5734).ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                if (v.Categories == null || v.Categories.Count == 0)
                {
                    FourSquare.SharpSquare.Entities.Venue venSquare = sharpSquare.GetVenue(v.SquareId);
                    if (venSquare.categories != null)
                    {
                        foreach (FourSquare.SharpSquare.Entities.Category c in venSquare.categories)
                        {
                            Banco.Models.Category cat;
                            cat = db.Categories.FirstOrDefault(f => f.SquareId == c.id);
                            if (cat == null)
                            {
                                cat = new Banco.Models.Category();
                                cat.Name = c.name;
                                cat.SquareId = c.id;
                                v.Categories.Add(cat);
                            }
                            else
                            {
                                if (v.Categories.Where(w => w.SquareId == c.id) == null || v.Categories.Where(w => w.SquareId == c.id).Count() == 0)
                                {
                                    v.Categories.Add(cat);
                                }

                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            ViewBag.Message = "Preenche Categorias dos estabelecimentos.";
            return View();
        }

        public ActionResult PreencherUsuarios()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            BancoContext db = new BancoContext();
            int lastid = 13027;
            Dictionary<string, string> parametros = new Dictionary<string, string>();

            List<Banco.Models.User> lisUser = db.Users.Where(w => w.Sexo == null && w.Id > lastid).ToList();

            foreach (Banco.Models.User usuario in lisUser)
            {
                try
                {
                    FourSquare.SharpSquare.Entities.User us = new FourSquare.SharpSquare.Entities.User();
                    lastid = usuario.Id;
                    us = sharpSquare.GetUser(usuario.SquareId);
                    usuario.Sexo = us.gender;
                    usuario.countAmigos = (int)us.friends.count;
                    usuario.countCheckin = (int)us.checkins.count;
                    usuario.countTip = (int)us.tips.count;
                    usuario.cidadeNatal = us.homeCity;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    if (e.Message == "O servidor remoto retornou um erro: (403) Proibido.")
                    {
                        break;
                    }
                }
            }
            return View();
        }
    }
}