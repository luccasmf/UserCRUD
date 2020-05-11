using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserCRUD.ViewModels
{
    public class ObjectResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ObjectDataResult<T> : ObjectResult
    {
        public T Data { get; set; }
    }
}
