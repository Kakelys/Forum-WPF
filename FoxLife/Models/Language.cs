using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;

namespace FoxLife.Models
{
    class Language:ViewModelBase
    {
        public string Name { get; private set; }
        public CultureInfo Culture { get; private set; }


        public static List<Language> Load()
        {
            var languages = new List<Language>
            {
                new() {Name = "English", Culture = new CultureInfo("eu-US")},
                new() {Name = "Русский", Culture = new CultureInfo("ru-RU")}
            };

            return languages;
        }
    }
}
