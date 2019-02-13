using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gestionaMelon.Helpers
{
    public class CustomHelpers
    {
        public static SelectList SelectListForBoolean(object selectedValue = null)
        {
            SelectListItem[] selectListItems = new SelectListItem[2];

            var itemTrue = new SelectListItem();
            itemTrue.Value = "true";
            itemTrue.Text = "Yes";
            selectListItems[0] = itemTrue;

            var itemFalse = new SelectListItem();
            itemFalse.Value = "false";
            itemFalse.Text = "No";
            selectListItems[1] = itemFalse;

            var selectList = new SelectList(selectListItems, "Value", "Text", selectedValue);

            return selectList;
        }           
    }
}