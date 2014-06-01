using System;

namespace CsRopExample.DomainModels
{
    public class EmailAddressChangedEventArgs : EventArgs
    {
        public EmailAddress OldAddress { get; set; }
        public EmailAddress NewAddress { get; set; }
    }

    static class DomainEvents
    {
        public static event EventHandler<EmailAddressChangedEventArgs> EmailAddressChanged;

        public static void OnEmailAddressChanged(EmailAddress oldAddress, EmailAddress newAddress)
        {
            var handler = EmailAddressChanged;
            if (handler != null)
            {
                var args = new EmailAddressChangedEventArgs { OldAddress = oldAddress, NewAddress = newAddress };
                handler(null, args);
            }
        }
    }
}
