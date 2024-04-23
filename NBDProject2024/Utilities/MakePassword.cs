using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace NBDProject2024.Utilities
{
    public static class MakePassword
    { 
        public static string Generate(int NumberOfCharacters = 8)
        {
            var random = new Random();

            string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string Lowercase = "abcdefghijklmnopqrstuvwxyz";
            string Digit = "0123456789";
            string NonAlphanumeric = "-._@+";
            string AllCharacters = Lowercase + Uppercase
                + Digit + NonAlphanumeric;

            var charsCount= AllCharacters.Length; 

            var password = new char[NumberOfCharacters];

            password[0] = Uppercase[random.Next(0, Uppercase.Length)];
            password[1] = Lowercase[random.Next(0,Lowercase.Length)];
            password[2]= Digit[random.Next(0,Digit.Length)];
            password[3]= NonAlphanumeric[random.Next(0,NonAlphanumeric.Length)];

            for(int i =4; i < NumberOfCharacters; i++)
            {
                password[i] = AllCharacters[random.Next(charsCount)];
            }

            return new string(password);
        }
    }
}
