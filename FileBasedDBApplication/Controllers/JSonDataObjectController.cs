using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileBasedDBApplication.Models;

namespace FileBasedDBApplication.Controllers
{
    public class JSonDataObjectController : Controller
    {
        DatabaseAccess db = new DatabaseAccess();
        public ActionResult Index()
        {
            return View(db.GetAllJSonDatas());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(JsonStorageTable model)
        {
            if (ModelState.IsValid)
            {
                db.InserJsonData(model);
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Details(string id)
        {
            return View(db.GetAJSonData(id));
        }
        [HttpPost]
        public ActionResult Details(JsonStorageTable model)
        {
           
                JsonStorageTable dbModel = db.GetAJSonData(model.Key);

                if (db.DeleteKey(dbModel))
                 return  RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Key is not active to delete");

            return RedirectToAction("Index");
        }
    }
}