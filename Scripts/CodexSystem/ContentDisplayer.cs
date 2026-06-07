using System;
using System.ComponentModel.DataAnnotations;
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
    
    [ExportGroup("Video Settings")]
    [Export] VideoLayout videoLayout;
    [Export] PackedScene playButton;


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
                PortraitImage.Texture = information.CharacterPortrait;
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

                if (information.Video)
                {
                    Button instance = (Button)playButton.Instantiate();
                    MarginContainer margin = new();
                    margin.AddThemeConstantOverride("margin_top", 20);
                    SupportImages.AddChild(margin);
                    SupportImages.AddChild(instance);
                    instance.Pressed += () => PlayVideo(information.videoStream);
                }


                foreach (var image in information.SupportImages)
                {
                    MarginContainer margin = new();
                    margin.AddThemeConstantOverride("margin_top", 20);
                    SupportImages.AddChild(margin);

                    var textureRect = new TextureRect
                    {
                        Texture = image,
                        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                        StretchMode = TextureRect.StretchModeEnum.Keep
                    };

                    SupportImages.AddChild(textureRect);
                }
                break;
        }
    }

    void PlayVideo(VideoStream videoStream)
    {
        videoLayout.StartVideo(videoStream);
    }

    public void Clean()
    {
        TitleTerms.Text = "";
        ContentTerms.Text = "";
        TitleCharacter.Text = "";
        SubTitle.Text = "";
    }
}