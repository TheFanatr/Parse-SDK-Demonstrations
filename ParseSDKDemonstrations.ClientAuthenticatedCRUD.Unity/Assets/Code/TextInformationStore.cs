using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code
{
    [ParseClassName("TextStore")]
    public class TextInformationStore : ParseObject
    {
        [ParseFieldName("data")]
        public string Data
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
    }
}
