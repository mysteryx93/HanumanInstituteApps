namespace HanumanInstitute.Apps.AdRotator;

public class AdRotatorViewModelDesign : AdRotatorViewModel
{
    public static AdRotatorViewModelDesign Instance => new(); 
    
    public AdRotatorViewModelDesign() : base(null!, null!, null!, null!, null!, null!)
    {
        Enabled = false;
        Current = new AdItem()
        {
            Markdown = @"Do you want to improve your finances?
**Get a God & Money Reading**
Measure the aspects of your personality that have the most impact on money"
        };
    }
}
