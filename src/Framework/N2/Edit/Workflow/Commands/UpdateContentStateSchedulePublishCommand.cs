using System.Linq;
namespace N2.Edit.Workflow.Commands
{
    public class UpdateContentStateSchedulePublishCommand : CommandBase<CommandContext>
    {
        StateChanger changer;

        public UpdateContentStateSchedulePublishCommand(StateChanger changer)
        {
            this.changer = changer;
        }

        public override void Process(CommandContext state)
        {
            // items with a future publish date should have a 'waiting' state
            var toState = state.Content.Published.HasValue && state.Content.Published > N2.Utility.CurrentTime() ? ContentState.Waiting : ContentState.Published;

            changer.ChangeTo(state.Content, toState);
            foreach (ContentItem item in state.GetItemsToSave().Distinct())
            {
                if (item == state.Content)
                    continue;

                changer.ChangeTo(item, toState);
            }
        }
    }
}
