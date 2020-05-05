using Parse;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;

namespace ParseSDKDemonstrations.ClientAuthenticatedCRUD.Console
{
    class Program
    {
        delegate bool Parser<T>(string value, out T data);

        static Dictionary<string, string> Details { get; } = new Dictionary<string, string> { };

        static Dictionary<string, object> Values { get; } = new Dictionary<string, object> { };

        static void Get(params string[] names)
        {
            foreach (string name in names)
            {
                Get(name);
            }
        }

        static string Get(string name)
        {
            Write($"\t{name}: ");
            return (Details[name] = ReadLine()) as string;
        }
        
        static T Get<T>(string name, Parser<T> reader)
        {
            Write($"\t{name}: ");
            return (T) (Values[name] = reader.Invoke(ReadLine(), out T data) ? data : throw new Exception("Malformed data."));
        }

        static void ShowDataCollectionHeader(string name, bool distance = true)
        {
            if (distance)
            {
                WriteLine();
            }

            WriteLine($"Provide the {name}:");
            WriteLine();
        }

        static void ShowStatus(string status, bool distance = true)
        {
            if (distance)
            {
                WriteLine();
            }

            WriteLine($"{status}...");
        }

        static void ShowLabel(string name)
        {
            WriteLine();
            Write($"{name}: ");
        }

        static async Task Main()
        {
            ShowStatus("Connecting", false);

            ShowDataCollectionHeader("Connection Data");

            // Instantiate a ParseClient.
            ParseClient client = new ParseClient(Get("ID"), Get("Parse Server URI"), Get(".NET Key"));

            ShowStatus("Authenticating");

            // Authenticate the user.
            await GetAuthenticationDetails();

            ShowLabel("Current Session Token");

            // Get the authenticated user. This is can also be done with a variable that stores the ParseUser instance before the SignUp overload that accepts a ParseUser is called.
            WriteLine(client.GetCurrentUser().SessionToken);

            ShowStatus("Deauthenticating and Reauthenticating");

            // Deauthenticate the user.
            await client.LogOutAsync();

            // Authenticate the user.
            await GetAuthenticationDetails();

            ParseUser user = client.GetCurrentUser();

            ShowStatus("Testing");

            ShowDataCollectionHeader("Test Data");

            // Create a new object with permessions that allow only the user to modify it. Bind the object to the target ParseClient instance. This is unnecessary if Publicize is called on the client, as that makes the client the singleton instance, accessible via ParseClient.Instance, so it is bound to new ParseObjects by default.
            ParseObject testObject = new ParseObject(Get("Class Name"), client) { ACL = new ParseACL(user) };

            // Set some value on the object.
            testObject.Set(Get("Field Name"), Get("Value"));

            ShowLabel("Test Data ID Before Save");

            // See that the ObjectId of an unsaved object is null;
            WriteLine(testObject.ObjectId);

            // Save the object to the target Parse Server instance.
            await testObject.SaveAsync();

            ShowLabel("Test Data ID After Save");

            // See that the ObjectId of a saved object is non-null;
            WriteLine(testObject.ObjectId);

            ShowLabel($"{Details["Field Name"]} Value On Server");

            // Query the object back down from the server to check that it was actually saved.
            WriteLine((await client.GetQuery(Details["Class Name"]).WhereEqualTo("objectId", testObject.ObjectId).FirstAsync()).Get<string>(Details["Field Name"]));

            ShowStatus("Updating");

            ShowDataCollectionHeader("New Test Data");

            // Mutate some value on the object.
            testObject.Set(Details["Field Name"], Get("New Value"));

            // Save the object again.
            await testObject.SaveAsync();

            ShowLabel($"New {Details["Field Name"]} Value On Server");

            // Query the object again to see that the change was made.
            WriteLine((await client.GetQuery(Details["Class Name"]).WhereEqualTo("objectId", testObject.ObjectId).FirstAsync()).Get<string>(Details["Field Name"]));

            // Store the object's objectId so it can be verified that it was deleted later.
            string testObjectId = testObject.ObjectId;

            ShowStatus("Deleting");

            // Delete the object.
            await testObject.DeleteAsync();

            ShowLabel("Test Data Present On Server After Deletion");

            // Check that the object was deleted from the server.
            WriteLine(await client.GetQuery(Details["Class Name"]).WhereEqualTo("objectId", testObjectId).FirstOrDefaultAsync() is { });

            // Deauthenticate the user again.
            await client.LogOutAsync();

            async Task GetAuthenticationDetails()
            {
                ShowDataCollectionHeader("Handle and Password");

                Get("Handle", "Password");

                if (Get<bool>("Fresh Account", Boolean.TryParse))
                {
                    // Create a user, save it, and authenticate with it.
                    await client.SignUpAsync(Details["Handle"], Details["Password"]);
                }
                else
                {
                    // Authenticate the user.
                    await client.LogInAsync(Details["Handle"], Details["Password"]);
                }
            }
        }
    }
}
