using UnityEngine.UI;

public class ScrollRectFix : ScrollRect
{

    override protected void LateUpdate()
    {

        base.LateUpdate();

        if (this.horizontalScrollbar)
        {
            this.horizontalScrollbar.size = 0;
        }
        if (this.verticalScrollbar)
        {
            this.verticalScrollbar.size = 0;
        }
    }

    override public void Rebuild(CanvasUpdate executing)
    {

        base.Rebuild(executing);

        if (this.horizontalScrollbar)
        {
            this.horizontalScrollbar.size = 0;
        }
        if (this.verticalScrollbar)
        {
            this.verticalScrollbar.size = 0;
        }
    }
}