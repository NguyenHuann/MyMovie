using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MyMovie.Models
{
    // Thông báo mang theo giá trị string là tên người dùng mới
    public class UsernameChangedMessage : ValueChangedMessage<string>
    {
        public UsernameChangedMessage(string value) : base(value)
        {
        }
    }
}
