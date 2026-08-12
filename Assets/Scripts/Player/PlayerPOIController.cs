using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

public class PlayerPOIController
{
    private int totalPOI;
    private int visitedPOI;

    private PointOfInterest currentPOI;
    private Transform player;

    public PointOfInterest CurrentPOI => currentPOI;
    [CreateProperty] public int TotalPOI => totalPOI;
    [CreateProperty] public int VisitedPOI => visitedPOI;

    private Dictionary<PointOfInterest, bool> points;

    private PlayerHUD playerHUD;
    private PlayerArrow arrow;

    public PlayerPOIController(List<PointOfInterest> pois, Transform player, PlayerHUD hud, PlayerArrow arrow)
    {
        visitedPOI = 0;
        currentPOI = pois[0];
        totalPOI = pois.Count;
        this.player = player;
        playerHUD = hud;
        points = new Dictionary<PointOfInterest, bool>();
        foreach (PointOfInterest poi in pois)
        {
            points.Add(poi, false);
            poi.OnEnterPOI += HandleEnterPOI;
            poi.OnExitPOI += HandleExitPOI;
        }
        Debug.LogError("UPDATE PLAYER HUD VALUES");
        playerHUD.Init(this);
        //playerHUD.UpdateCurrentPOI(visitedPOI);
        //playerHUD.UpdateTotalPOI(totalPOI);
        this.arrow = arrow;
        this.arrow.Init(this);
    }

    public Quaternion GetArrowRotation()
    {
        if (currentPOI == null)
        {
            //SetArrowInvisible
            return Quaternion.identity;
        }

        var direction = currentPOI.transform.position - player.position;
        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void HandleEnterPOI(PointOfInterest poi, PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        arrow.SetVisible(false);
        if (points.ContainsKey(poi) && points[poi] == false)
        {
            points[poi] = true;
            UpdateCurrentPOI();
        }

    }

    private void UpdateCurrentPOI()
    {
        visitedPOI = points.Where(x => x.Value).Count();
        if (visitedPOI >= totalPOI)
            currentPOI = null;
        else
            currentPOI = points.FirstOrDefault(x => x.Value == false).Key;
    }
    private void HandleExitPOI(PointOfInterest poi, PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        arrow.SetVisible(visitedPOI < totalPOI);
    }


}
