using System;
using System.Collections.Generic;
using System.Text;

namespace sdakccapi.Dtos
{
    public class ResultObject
    {
        public string ReturnCode { get; set; }
        public string ReturnDescription { get; set; }
        public string Response { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
    }
}