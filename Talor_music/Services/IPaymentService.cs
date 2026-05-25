//namespace Talor_music.Services
//{
//    public interface IPaymentService
//    {
//        bool ValidatePayment(string cardNumber, string cvv, string expiry);
//    }

//    public class MockPaymentService : IPaymentService
//    {
//        // זו ה"רשימה הסודית" של ה-API שלנו
//        private readonly Dictionary<string, (string CVV, string Expiry)> _validCards = new()
//        {
//            { "4580123456789012", ("123", "12/28") }, // כרטיס ויזה דמה
//            { "5326123456789012", ("456", "05/27") }, // כרטיס מאסטרקארד דמה
//            { "1111222233334444", ("999", "01/30") }  // כרטיס של "טיילור" לבדיקות
//        };

//        public bool ValidatePayment(string cardNumber, string cvv, string expiry)
//        {
//            // ניקוי רווחים או מקפים אם המשתמש הזין
//            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");

//            // בדיקה האם הכרטיס קיים ברשימה והאם הפרטים תואמים
//            if (_validCards.TryGetValue(cleanNumber, out var details))
//            {
//                return details.CVV == cvv && details.Expiry == expiry;
//            }

//            return false; // הכרטיס לא קיים או שהפרטים שגויים
//        }
//    }
//}