using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

        public static int CalculateDuration(string testText, int wpm = 40)
        {
            if (string.IsNullOrEmpty(testText))
            {
                throw new ArgumentException("Test text cannot be null or empty.");
            }

            // Total character count
            int characterCount = testText.Length;

            // Calculate time in seconds using the formula: (Total characters / (5 * WPM)) * 60
            int timeInSeconds = Math.Max(((int)characterCount / (5 * wpm)) * 60, 5);

            return timeInSeconds;
        }

    }
}
