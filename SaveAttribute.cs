using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppChooser
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SaveAttribute : Attribute
    {
        public SaveAttribute()
        {

        }
    }
}
