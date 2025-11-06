using Metaplay.Core.Forms;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Localization;
using Metaplay.Core.Model;

namespace Game.Logic.Events
{
    [MetaSerializable]
    public enum Theme
    {
        Tutorial,
        Day,
        NightTime,
    }

    class ThemeValidator : MetaFormValidator<Theme>
    {
        public override void Validate(Theme fieldOrForm, FormValidationContext ctx)
        {
            if (fieldOrForm == Theme.Day || fieldOrForm == Theme.Tutorial)
                ctx.Fail($"{fieldOrForm} is enabled by default.");
        }
    }

    #region theme_content
    [LiveOpsEvent(1)]
    public class ThemeEvent : LiveOpsEventContent
    {
        [MetaMember(1)]
        [MetaFormDisplayProps("Theme", DisplayHint = "Which theme to enable for the users")]
        [MetaFormFieldCustomValidator(typeof(ThemeValidator))]
        public Theme Theme { get; private set; } = Theme.NightTime;

        [MetaMember(2)]
        [MetaFormDisplayProps("Perma Unlock Threshold", DisplayHint = "After how many runs the player will permanently unlock this theme")]
        public int PermaUnlockThreshold = 10;

        [MetaMember(3)]
        [MetaFormDisplayProps("Event In-Game Title", DisplayHint = "Event title in Game UI")]
        public string EventInGameTitle;
    }
    #endregion theme_content

    #region theme_event_model
    [MetaSerializableDerived(1)]
    public partial class ThemeEventModel : PlayerLiveOpsEventModel<ThemeEvent, PlayerModel>
    {
        [MetaMember(1)] public int RunsInThemeSinceStart { get; private set; }

        [MetaMember(2)] public bool RewardGiven { get; private set; }

        public void IncrementRunAndReward(PlayerModel player, string themeUsed)
        {
            if (Content.Theme.ToString() == themeUsed)
            {
                RunsInThemeSinceStart++;

                if (RunsInThemeSinceStart >= Content.PermaUnlockThreshold && !RewardGiven)
                {
                    RewardGiven = true;
                    if (!player.PlayerData.Themes.Contains(themeUsed))
                    {
                        player.PlayerData.Themes.Add(themeUsed);
                    }
                }
            }
        }
    }
    #endregion theme_event_model

    #region phase_changed
    public partial class ThemeEventModel : PlayerLiveOpsEventModel<ThemeEvent, PlayerModel>
    {
        protected override void OnPhaseChanged(PlayerModel player, LiveOpsEventPhase oldPhase, LiveOpsEventPhase[] fastForwardedPhases,
            LiveOpsEventPhase newPhase)
        {
            player.ClientListener.ThemeEventChanged();
        }
    }
    #endregion phase_changed
}