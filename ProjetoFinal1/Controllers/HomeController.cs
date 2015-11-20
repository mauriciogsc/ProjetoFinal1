
using FourSquare.SharpSquare.Core;
using FourSquare.SharpSquare.Entities;
using System;
using Banco.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetoFinal1.Controllers
{
    public class HomeController : Controller
    {
        string clientId = "V0LRK1PVS30TMTHFQNUFWCZC5C4ZBE4J02O5EXF4O421FIAI";
        string clientSecret = "REP3R12EZYWKOA0PPTMBSFSU5SRGS1UBV2VZVVOW2VIWGZB0";
        string redirectUri = "REDIRECT_URI";
        BancoContext db = new BancoContext();

        public ActionResult Index()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            List<Banco.Models.Venue> venues = db.Venues.ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                FourSquare.SharpSquare.Entities.Venue venSquare = sharpSquare.GetVenue(v.SquareId);
                if (venSquare.menu != null)
                    v.HasMenu = true;
                db.SaveChanges();
            }
            return View();
        }

        public ActionResult About()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            List<Banco.Models.Tip> tiplist = new List<Banco.Models.Tip>();
            List<Banco.Models.User> userlist = new List<Banco.Models.User>();
            List<Banco.Models.Venue> venuelist = new List<Banco.Models.Venue>();
            Dictionary<string, string> parametros = new Dictionary<string, string>();
            List<FourSquare.SharpSquare.Entities.Venue> venues = new List<FourSquare.SharpSquare.Entities.Venue>();
            List<FourSquare.SharpSquare.Entities.Tip> tips = new List<FourSquare.SharpSquare.Entities.Tip>();
            parametros.Add("limit", "500"); // tentando pegar ateh 500 venues e tips
            for (double lat = -22.9856; lat < -22.9452; lat += 0.0001)
            {
                parametros.Add("ll",lat.ToString().Replace(',', '.') +",-43.1900" );
                venues = sharpSquare.SearchVenues(parametros);

                foreach (FourSquare.SharpSquare.Entities.Venue v in venues)
                {
                    Banco.Models.Venue ven;
                    ven = db.Venues.FirstOrDefault(f => f.SquareId == v.id);
                    if (ven == null && venuelist.FirstOrDefault(f => f.SquareId == v.id) == null)
                    {
                        ven = new Banco.Models.Venue();
                        ven.SquareId = v.id;
                        venuelist.Add(ven);
                        db.Venues.Add(ven);
                    }
                    ven.Name = v.name;
                    parametros.Remove("ll");
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
                        if (user == null && userlist.FirstOrDefault(f=>f.SquareId == t.user.id)==null)
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
            ViewBag.Message = "Venues, tips e users para a coordenada -22.9843,-43.2018 adicionados ao banco com sucesso.";
            return View();
        }

        public ActionResult Contact()
        {
            SharpSquare sharpSquare = new SharpSquare(clientId, clientSecret);
            List<Banco.Models.Venue> venues = db.Venues.Include("Categories").ToList();
            foreach (Banco.Models.Venue v in venues)
            {
                FourSquare.SharpSquare.Entities.Venue venSquare = sharpSquare.GetVenue(v.SquareId);
                if(venSquare.categories!=null)
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
                            if (v.Categories.Where(w => w.SquareId == c.id) == null || v.Categories.Where(w => w.SquareId == c.id).Count()==0)
                            {
                                v.Categories.Add(cat);
                            }

                        }
                    }
                }
                db.SaveChanges();
            }
            ViewBag.Message = "Venues, tips e users para a coordenada -22.9843,-43.2018 adicionados ao banco com sucesso.";
            return View();
        }
    }
}