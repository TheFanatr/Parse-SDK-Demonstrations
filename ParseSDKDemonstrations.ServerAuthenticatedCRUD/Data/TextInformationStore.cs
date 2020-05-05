using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParseSDKDemonstrations.ServerAuthenticatedCRUD.Data
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
