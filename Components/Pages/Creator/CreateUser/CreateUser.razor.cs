namespace USPSimGame.Components.Pages.Creator.CreateUser
{
  public partial class CreateUser
  {

    private ChipDog? MyDog { get; set; }

    private bool IsSend { get; set; }

    protected override void OnInitialized() => MyDog ??= new(); // 

    private void LogicFunction()
    {
      logger.LogInformation($"Id = {MyDog?.Id} is null: {MyDog?.Id is null}");
      if (MyDog?.Id is not null) IsSend = true;

    }

    public class ChipDog
    {
      public string? Id { get; set; }
    }
  }
}