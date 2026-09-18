using Microsoft.EntityFrameworkCore;
using PqsTracker.Models;

namespace PqsTracker.Data;

// Sample content only. Not derived from any organization's actual PQS
// program, and not intended to represent one.
public static class SeedData
{
    public static async Task SeedAsync(PqsDbContext db)
    {
        if (await db.Qualifications.AnyAsync())
            return;

        var qualification = new Qualification
        {
            Name = "Reactor Operator",
            Description = "Sample qualification for demonstration purposes only; not derived from any organization's actual PQS program.",
            LineItems =
            [
                new LineItem { Section = Section.Fundamentals, Number = "101.1", Description = "Discuss the purpose and administrative requirements of the PQS program." },
                new LineItem { Section = Section.Fundamentals, Number = "102.1", Description = "Explain the fundamentals of heat transfer and fluid flow relevant to reactor plant operation." },
                new LineItem { Section = Section.Fundamentals, Number = "103.1", Description = "Explain the principles of reactor physics, including reactivity and neutron population control." },
                new LineItem { Section = Section.Fundamentals, Number = "104.1", Description = "Describe basic radiological control principles and dose limits." },
                new LineItem { Section = Section.Fundamentals, Number = "105.1", Description = "Explain the administrative and technical requirements for conduct of operations." },

                new LineItem { Section = Section.Systems, Number = "201.1", Description = "Trace the flowpath of the reactor coolant system and describe its major components." },
                new LineItem { Section = Section.Systems, Number = "202.1", Description = "Trace the flowpath of the chemical and volume control system." },
                new LineItem { Section = Section.Systems, Number = "203.1", Description = "Explain the function and operation of the residual heat removal system." },
                new LineItem { Section = Section.Systems, Number = "204.1", Description = "Describe the electrical distribution system supporting reactor plant equipment." },
                new LineItem { Section = Section.Systems, Number = "205.1", Description = "Explain the function and alignment of the component cooling water system." },
                new LineItem { Section = Section.Systems, Number = "206.1", Description = "Describe the design and operation of the reactor protection system." },

                new LineItem { Section = Section.Watchstations, Number = "301.1", Description = "Stand a supervised watch as reactor operator under normal operating conditions." },
                new LineItem { Section = Section.Watchstations, Number = "302.1", Description = "Respond to a simulated reactor scram in accordance with abnormal operating procedures." },
                new LineItem { Section = Section.Watchstations, Number = "303.1", Description = "Perform a normal reactor plant startup under instruction." },
                new LineItem { Section = Section.Watchstations, Number = "304.1", Description = "Perform a normal reactor plant shutdown under instruction, as an advanced/optional watch task.", IsRequired = false },
            ]
        };

        db.Qualifications.Add(qualification);
        await db.SaveChangesAsync();
    }
}
