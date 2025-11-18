using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Or.Business
{
    // Projet or - partie 1 : ajouter le type de transaction dans détails compte
    public class TypeTransacConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Operation ope)
            {
                switch (ope)
                {
                    case Operation.DepotSimple:
                        {
                            return "Dépôt";                         
                        }
                    case Operation.RetraitSimple:
                        {
                            return "Retrait";
                        }
                    case Operation.InterCompte:
                        {
                            return "Virement";
                        }
                    default:
                        {
                            return "Opération non référencée";
                        }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Pas besoin en lecture seule
        }
    }
}
