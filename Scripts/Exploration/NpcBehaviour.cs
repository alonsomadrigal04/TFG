using Godot;
using System;

public partial class NpcBehaviour : RigidBody3D, IInteractable
{
	const float VisualScaleCompensation = 1f;

	[ExportGroup("CHARACTER INFO")]
	public NpcData CharacterInfo { get; private set; }
	public void Initialize(NpcData data)
    {
        CharacterInfo = data;
    }

	[ExportGroup("REFERENCES")]
	[Export] Texture2D dialogImg;
	[Export] Sprite3D characterSprite;
	[Export] CollisionShape3D colliderBody;
	[Export] CollisionShape3D colliderInteract;
	[Export] InteractIconAnimation dialogIcon;
	[Export] Area3D interactionArea;
	[Export] float interactionMargin = 2f;

	public int priority { get; set; } = 5;
	public bool HasDialog { get; private set; } = true;

	Vector2 spriteSize;

	public override void _Ready()
	{
		if (!ValidateReferences()) return;

		if (!ValidateCharacterInfo()) return;

		spriteSize = CalculateSpriteSize();
		ConfigureVisual(spriteSize);
		ConfigureCollider(spriteSize);
		ConfigureInteractionArea(spriteSize);
		ConfigureInteractIcon(spriteSize.Y);
		SnapToGround();
	}

	void ConfigureNPC()
	{
		
	}


	void SnapToGround()
	{
		var spaceState = GetWorld3D().DirectSpaceState;

		Vector3 from = GlobalPosition + Vector3.Up * 5f;
		Vector3 to = GlobalPosition + Vector3.Down * 50f;

		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = false;

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			Vector3 hitPoint = (Vector3)result["position"];
			float halfHeight = spriteSize.Y / 2f;
			GlobalPosition = new Vector3(
				GlobalPosition.X,
				hitPoint.Y + halfHeight,
				GlobalPosition.Z
			);
		}
		else
		{
			GD.PrintErr("[NpcBehaviour] Ground not detected below NPC.");
		}
	}

	public override void _ExitTree()
	{
		if (interactionArea != null)
		{
			interactionArea.BodyEntered -= OnBodyEntered;
			interactionArea.BodyExited -= OnBodyExited;
		}
	}

	bool ValidateReferences()
	{
		if (characterSprite == null || colliderBody == null || dialogIcon == null || interactionArea == null)
		{
			GD.PrintErr("[NpcBehaviour] Missing exported references.");
			return false;
		}

		return true;
	}

	bool ValidateCharacterInfo()
	{
		if (CharacterInfo == null)
		{
			GD.PrintErr("[NpcBehaviour] CharacterInfo is null.");
			return false;
		}

		if (CharacterInfo.WordPortrait == null)
		{
			GD.PrintErr($"[NpcBehaviour] {CharacterInfo} has no WordPortrait.");
			return false;
		}

		return true;
	}

	Vector2 CalculateSpriteSize()
	{
		float width = CharacterInfo.WordPortrait.GetWidth() *
					  characterSprite.PixelSize *
					  characterSprite.Scale.X;

		float height = CharacterInfo.WordPortrait.GetHeight() *
					   characterSprite.PixelSize *
					   characterSprite.Scale.Y;

		return new Vector2(width, height) * VisualScaleCompensation;
	}

	void ConfigureVisual(Vector2 size)
	{
		characterSprite.Texture = CharacterInfo.WordPortrait;
		characterSprite.Position = new Vector3(0f, size.Y / 2f, 0f);
	}

	void ConfigureCollider(Vector2 size)
	{
		colliderBody.Position = new Vector3(0f, size.Y / 2f, 0f);

		if (colliderBody.Shape is CylinderShape3D cylinder)
		{
			cylinder.Radius = size.X / 2f;
			cylinder.Height = size.Y;
		}
	}

	void ConfigureInteractionArea(Vector2 size)
	{
		if (colliderInteract?.Shape is CylinderShape3D cylinder)
		{
			cylinder.Radius = size.X / 2f + interactionMargin;
			cylinder.Height = size.Y;
		}

		interactionArea.BodyEntered += OnBodyEntered;
		interactionArea.BodyExited += OnBodyExited;
	}

	void ConfigureInteractIcon(float height)
	{
		dialogIcon.InitializeValues(height, HasDialog, dialogImg);
	}

	public void Interact()
	{
		if (!HasDialog) return;
		GameStateManager.Instance.ChangeState(State.Dialog, this);
	}

	void OnBodyEntered(Node3D body)
	{
		if (body is PlayerBehaviour player)
		{
			// dialogIcon.Activate();
			player.RegisterInteractable(this);
		}
	}

	void OnBodyExited(Node3D body)
	{
		if (body is PlayerBehaviour player)
		{
			// dialogIcon.Desactivate();
			player.UnregisterInteractable(this);
		}
	}

	public void SetDialogState(bool hasDialog)
	{
		HasDialog = hasDialog;
		//dialogIcon.UpdateState(hasDialog);
	}
}
