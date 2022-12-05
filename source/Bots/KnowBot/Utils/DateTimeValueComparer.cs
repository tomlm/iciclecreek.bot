using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KnowBot.Dialogs
{
    internal class DateTimeValueComparer : EqualityComparer<DateTimexValue>
    {
        public override bool Equals([AllowNull] DateTimexValue x, [AllowNull] DateTimexValue y) => JObject.FromObject(x).ToString() == JObject.FromObject(y).ToString();

        public override int GetHashCode([DisallowNull] DateTimexValue obj) => JObject.FromObject(obj).ToString().GetHashCode();
    }
}
