
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
            var tips = db.Tips.Where(w=>w.AlchemyPredict == 0 && w.status != 0);
            foreach (Banco.Models.Tip tip  in tips)
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

        public ActionResult Satisfacao()
        {
            List<Banco.Models.User> users = db.Users.ToList();

            //números obtidos pelo sql server para essa amostra
            double desviopadrao = 1.67239257986971;
            double mediaTotal = 7.92828871470173;
            foreach (Banco.Models.User u in users)
            {
                List<Banco.Models.Tip> tipsUser = db.Tips.Where(w => w.UserId == u.Id && w.WekaPredictFinal!=0).ToList();
                int somaComentarios = 0;
                foreach(Banco.Models.Tip t in tipsUser)
                {
                    if(t.WekaPredictFinal ==1)
                    {
                        somaComentarios += 10;
                    }
                    else if (t.WekaPredictFinal==2)
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
            foreach(Banco.Models.Venue v in venues)
            {
                List<Banco.Models.Tip> tipsVenue = db.Tips.Where(w => w.VenueId == v.Id).ToList();
                float somaPesos=0;
                float somaTotal =0;
                foreach(Banco.Models.Tip t in tipsVenue)
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
                    v.rateWeka = (double) somaTotal / (double)somaPesos;
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
            foreach(Banco.Models.Tip t in tips)
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
            var qry = db.Tips.Where(t => t.status == 0 && t.Venue.tipcount >9).OrderBy(o=>o.Id);
           

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