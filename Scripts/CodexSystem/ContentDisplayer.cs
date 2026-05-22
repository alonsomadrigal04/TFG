using Godot;

public partial class ContentDisplayer : Control
{
    [ExportGroup("Character Displayer")]
    [Export] Control CharacterLayout;    
    [Export] Label TitleCharacter;
    [Export] Label SubTitle;
    [Export] TextureRect PortraitImage;
    [Export] Label ContentCharacter;

    [ExportGroup("Others Displayer")]
    [Export] Control TermsLayout;
    [Export] Label TitleTerms;
    [Export] Label ContentTerms;
    [Export] VBoxContainer SupportImages;

    public void DisplayContent(CoreInformation information)
    {
        switch (information.DisplayType)
        {
            case CodexType.Character:
                CharacterLayout.Show();
                TermsLayout.Hide();
                CharacterLayout.Visible = true;
                TermsLayout.Visible = false;

                TitleCharacter.Text = information.Title;
                SubTitle.Text = information.SubTitle;
                PortraitImage.Texture = (Texture2D)information.CharacterPortrait;
                ContentCharacter.Text = information.Content;
                break;

            case CodexType.Term:
            case CodexType.Localization:
                CharacterLayout.Hide();
                TermsLayout.Show();
                CharacterLayout.Visible = false;
                TermsLayout.Visible = true;

                TitleTerms.Text = information.Title;
                ContentTerms.Text = information.Content;

                foreach (var image in SupportImages.GetChildren())
                    image.QueueFree();

                foreach (var image in information.SupportImages)
                {
                    var textureRect = new TextureRect
                    {
                        Texture = (Texture2D)image
                    };
                    SupportImages.AddChild(textureRect);
                }
                break;
        }
    }



}