using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public static class ApplicationConstants
    {
        public const string AdminRoleName = "admin";
        public const string SellerRoleName = "seller";
        public const string UserRoleName = "user";

        public const int MinRating = 0;
        public const int MaxRating = 5;
    }
}