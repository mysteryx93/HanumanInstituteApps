namespace HanumanInstitute.Apps.AdRotator;

public class AdRotatorViewModelDesign : AdRotatorViewModel
{
    public static AdRotatorViewModelDesign Instance => new AdRotatorViewModelDesign(); 
    
    public AdRotatorViewModelDesign() : base(null!, null!, null!, null!, null!, null!)
    {
        Current = new AdItem()
        {
            Markdown = @"**Sign up for the Force of Life Free Training to**  
* See what is keeping you stuck so that you can turn the situation around  
* Understand the true power hidden within you  
* Open the door to stepping into your greater path, purpose and freedom"
        };
    }
}
