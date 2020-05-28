using Microsoft.Bot.Builder.Dialogs;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Dialogs.Annotations.Tests
{

    public class Foo : Dialog
    {
        public const string Kind = "Test.Foo";

        [Description("The quoted text")]
        [Entity("@quotedString", "\"this is quoted\"")]
        [RegularExpression("???-???-????")]
        public string QuotedText { get; set; }

        [Description("This is some data")]
        [Entity("@number", "1", "two", "three")]
        [Entity("@quantity", "1 lb", "two pounds", "three lbs")]
        public float Amount { get; set; }

        [Description("This is some data")]
        [DataType(DataType.Date)]
        [Entity("@datetimev2", "next week")]
        public string SomeDate { get; set; }

        [Description("This is a phone number")]
        [DataType(DataType.PhoneNumber)]
        [Entity("@phone", "425-123-1234", "555-555-5432")]
        [RegularExpression("???-???-????")]
        public string Phone { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, System.Object options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }

}
