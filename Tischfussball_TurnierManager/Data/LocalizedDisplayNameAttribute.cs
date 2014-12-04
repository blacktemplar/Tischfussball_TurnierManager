using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace Tischfussball_TurnierManager.Data
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private LocTextExtension locExtension;

        public LocalizedDisplayNameAttribute(string key, string dictName = "Resources", string assemblyName = null)
        {
            if (String.IsNullOrEmpty(assemblyName))
            {
                assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            }
            locExtension = new LocTextExtension(assemblyName + ":" + dictName + ":" + key);
        }

        public override string DisplayName
        {
            get
            {
                string res;
                locExtension.ResolveLocalizedValue(out res);
                return res;
            }
        }
    }
}