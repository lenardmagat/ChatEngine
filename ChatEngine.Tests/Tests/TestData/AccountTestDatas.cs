namespace ChatEngine.Tests.AccountDatas;
public class AccountTestDatas
{
    public static IEnumerable<object[]> CreateAccountData()
    {
        yield return new object[] { "testuser1", "TestPass123!", 200 };
        yield return new object[] { "testuser2", "TestPass123!", 200 }; 
        yield return new object[] {"test", "Test", 400};
        yield return new object[] {"test1231", "Test", 400};
        yield return new object[] {"testuser11231244123124123412312", "TestPass456!", 400};
        yield return new object[] {"", "TestPass789!", 400};
    }

    public static IEnumerable<object[]> LoginAccountData()
    {
        yield return new object[] { "testuser1", "TestPass123!" };
        yield return new object[] { "testuser2", "TestPass123!" };
        yield return new object[] { "testuser3", "TestPass789!" };
    }

    public static IEnumerable<object[]> ChangePasswordData()
    {
        yield return new object[] { 
            "testuser1",
            "TestPass123!", 
            "TestNewPass", 
            200, 
            };
        
        yield return new object[] { 
            "testuser2",
            "TestPass123!", 
            "TestNewPass", 
            401, 
            "asda", 
            };
        
        yield return new object[] { 
            "testuser3",
            "TestPass123!", 
            "TestNewPass", 
            401, 
            "asda"
            };
        
        yield return new object[] { 
            "testuser4",
            "TestPass123!", 
            "Test", 
            400, 
            };
    }

}