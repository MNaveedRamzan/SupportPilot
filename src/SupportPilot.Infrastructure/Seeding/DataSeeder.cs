using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;
using SupportPilot.Infrastructure.Persistence;

namespace SupportPilot.Infrastructure.Seeding;

/// <summary>
/// Populates the database with realistic demo data on first run, so a
/// recruiter or interviewer can explore SupportPilot immediately without
/// manual setup. Only runs if the Users table is empty — safe to call on
/// every startup, it's a no-op after the first successful seed.
/// </summary>
public static class DataSeeder
{
    private const string DemoPassword = "Demo1234!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<SupportPilotDbContext>();

        var alreadySeeded = await context.Users.AnyAsync();
        if (alreadySeeded)
        {
            return;
        }

        var admin = new User
        {
            Email = "admin@supportpilot.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
            Role = UserRole.Admin
        };

        var customer = new User
        {
            Email = "customer@supportpilot.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
            Role = UserRole.Customer
        };

        context.Users.AddRange(admin, customer);
        await context.SaveChangesAsync();

        await SeedKnowledgeBaseAsync(services);
        await SeedConversationsAndTicketsAsync(context);
    }

    private static async Task SeedKnowledgeBaseAsync(IServiceProvider services)
    {
        var kbService = services.GetRequiredService<IKnowledgeBaseService>();

        string[] articles =
        {
            "Refunds are processed within 5-7 business days after the return " +
            "is received at our warehouse. Refunds are issued to the original " +
            "payment method used at checkout.",

            "To reset your password, go to the login page and click 'Forgot " +
            "Password'. You'll receive an email with a reset link valid for 24 hours.",

            "Standard shipping takes 3-5 business days within the country. " +
            "Express shipping (2-day) is available at checkout for an additional fee.",

            "You can update your account email address from Account Settings. " +
            "A verification email will be sent to the new address before the change takes effect.",

            "Subscription plans can be upgraded or downgraded at any time from " +
            "the Billing section. Changes take effect at the start of the next billing cycle.",

            "If an order arrives damaged, contact support within 48 hours with " +
            "photos of the damage. We'll arrange a free replacement or full refund."
        };

        foreach (var article in articles)
        {
            await kbService.AddArticleAsync(article);
        }
    }

    private static async Task SeedConversationsAndTicketsAsync(SupportPilotDbContext context)
    {
        var resolvedConversation = new Conversation
        {
            IsEscalated = false,
            Messages =
            {
                new Message
                {
                    ConversationId = Guid.Empty, // set below after Id is known
                    Role = ChatRole.User,
                    Content = "How long does shipping usually take?",
                    SentimentScore = 0.1f
                },
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.Assistant,
                    Content = "Standard shipping takes 3-5 business days. Express " +
                              "shipping (2-day) is available at checkout for an additional fee."
                }
            }
        };

        var escalatedConversation1 = new Conversation
        {
            IsEscalated = true,
            Messages =
            {
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.User,
                    Content = "My order arrived completely smashed and this is the " +
                              "second time this has happened. I'm extremely frustrated.",
                    SentimentScore = 0.9f
                },
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.Assistant,
                    Content = "I'm really sorry to hear that. I've escalated this " +
                              "to our support team, who will reach out shortly to " +
                              "arrange a replacement."
                }
            }
        };

        var escalatedConversation2 = new Conversation
        {
            IsEscalated = true,
            Messages =
            {
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.User,
                    Content = "I've been charged twice for the same order and no " +
                              "one has responded to my emails. This is unacceptable.",
                    SentimentScore = 0.85f
                },
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.Assistant,
                    Content = "I understand the frustration — I've flagged this as " +
                              "a billing issue and created a ticket for our team to resolve it."
                }
            }
        };

        var casualConversation = new Conversation
        {
            IsEscalated = false,
            Messages =
            {
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.User,
                    Content = "Can I change my subscription plan mid-cycle?",
                    SentimentScore = 0.05f
                },
                new Message
                {
                    ConversationId = Guid.Empty,
                    Role = ChatRole.Assistant,
                    Content = "Yes, you can upgrade or downgrade anytime from the " +
                              "Billing section. Changes apply at the start of the next billing cycle."
                }
            }
        };

        context.Conversations.AddRange(
            resolvedConversation,
            escalatedConversation1,
            escalatedConversation2,
            casualConversation);

        await context.SaveChangesAsync();

        var ticket1 = new Ticket
        {
            Subject = "Damaged item received — repeat issue",
            Description = "Customer reports their order arrived damaged for the " +
                          "second time. Requires replacement or refund.",
            Status = TicketStatus.Open
        };

        var ticket2 = new Ticket
        {
            Subject = "Duplicate charge on order",
            Description = "Customer was billed twice for a single order. " +
                          "Needs billing team review and refund of the duplicate charge.",
            Status = TicketStatus.Open
        };

        context.Tickets.AddRange(ticket1, ticket2);
        await context.SaveChangesAsync();

        escalatedConversation1.LinkedTicketId = ticket1.Id;
        escalatedConversation2.LinkedTicketId = ticket2.Id;
        await context.SaveChangesAsync();
    }
}