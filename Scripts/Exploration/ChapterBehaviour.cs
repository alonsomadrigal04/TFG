using Godot;
using System;
[GlobalClass]
public partial class ChapterBehaviour : Node
{
    [Export] public ChapterData[] Chapters;
    public int CurrentChapter { get; private set; } = -1;

    public void ChangeChapter()
    {
        CurrentChapter++;

        if (CurrentChapter >= Chapters.Length)
        {
            GD.Print("No more chapters.");
            return;
        }

        GD.Print($"Starting chapter: {CurrentChapter}...\n");

        GameManager.ChangeEnvironment(GameEnvironments.ChapterEnv);

    }

    public void ChargeChapter()
    {
        ChapterData chapter = Chapters[CurrentChapter];

        switch(chapter.Type)
        {
            case ChapterType.Dialogue:
                GameManager.ChangeEnvironment(GameEnvironments.OnlyTextBox);
                break;

            case ChapterType.Exploration:
                GameManager.ChangeEnvironment(chapter.ExplorationEnvironment);
                break;
        }

        if (!string.IsNullOrEmpty(chapter.StartConversation))
        {
            GameManager.Instance.DialogManager.StartDialogScene(ConversationsDataBase.GetConversation(chapter.StartConversation));
        }
    }

    public string GetChapterName () => Chapters[CurrentChapter].Title;
    public string GetChapterSubName () => Chapters[CurrentChapter].Subtitle;

}