using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OMT.DataService.Utility
{
    public static class ValidationField
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Comprehensive email validation regex
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailRegex);
        }


        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Example regex for a 10-digit phone number (adjust as needed for your requirements)
            var phoneRegex = @"^\d{10}$";
            return Regex.IsMatch(phone, phoneRegex);
        }

        public static int CalculateDuration(string testText)
        {
            if (string.IsNullOrEmpty(testText))
            {
                // Return 0 if the text is null or empty
                throw new ArgumentException("Test text cannot be null or empty.");
            }

            // Split the text and calculate the number of words
            int wordCount = testText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;



            // Calculate the duration and round it
            return (int)Math.Round((double)(wordCount * 60) / 40);
        }

    }
}
