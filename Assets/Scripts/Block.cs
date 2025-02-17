using UnityEngine;

public class Block : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] icons; 
    private int colorIndex; 

    public GameManager gameManager; 

    
    public void SetColor(int colorIndex)
    {
        this.colorIndex = colorIndex;
        UpdateIcon(0);
    }

    
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

   
    public void UpdateIcon(int groupSize)
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager reference is not set for this block.");
            return;
        }

        int A = gameManager.A;
        int B = gameManager.B;
        int C = gameManager.C;

        if (groupSize > C) spriteRenderer.sprite = icons[3];
        else if (groupSize > B) spriteRenderer.sprite = icons[2];
        else if (groupSize > A) spriteRenderer.sprite = icons[1];
        else spriteRenderer.sprite = icons[0];
    }

   
    public int GetColor()
    {
        return colorIndex;
    }

    
    private void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.HandleBlockClick(this);
        }
    }
}
