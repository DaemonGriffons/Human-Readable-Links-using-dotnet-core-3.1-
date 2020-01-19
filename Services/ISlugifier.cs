using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanReadableLinks.Services
{
    public interface ISlugifier
    {
        string CreateSlug(string title);
        string GetLink(string slug);
    }
}
