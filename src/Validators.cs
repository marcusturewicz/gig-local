using System;
using System.ComponentModel.DataAnnotations;

namespace GigLocal
{
    public class FutureDate : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime d = Convert.ToDateTime(value);
            Console.WriteLine(d);
            return d >= DateTime.Now;
        }
    }
}