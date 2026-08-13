using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPOIController : INotifyBindablePropertyChanged
{
    private int totalPOI;
    private int visitedPOI;

    private PointOfInterest currentPOI;
    private Transform player;

    public PointOfInterest CurrentPOI => currentPOI;

    [CreateProperty] public int TotalPOI => totalPOI;
    [CreateProperty] public int VisitedPOI => visitedPOI;

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

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

        playerHUD.Init(this);

        this.arrow = arrow;
        this.arrow.Init(this);
    }

    public Quaternion GetArrowRotation()
    {
        if (currentPOI == null)
            return Quaternion.identity;

        var direction = currentPOI.transform.position - player.position;
        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void HandleEnterPOI(PointOfInterest poi, PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        arrow.SetVisible(false);

        if (points.TryGetValue(poi, out bool visited) && !visited)
        {
            points[poi] = true;
            visitedPOI = points.Count(x => x.Value);

            Notify(nameof(VisitedPOI));
        }
    }

    private void UpdateCurrentPOI()
    {
        if (visitedPOI >= totalPOI)
            currentPOI = null;
        else
            currentPOI = points.FirstOrDefault(x => !x.Value).Key;
    }

    private void HandleExitPOI(PointOfInterest poi, PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        UpdateCurrentPOI();
        arrow.SetVisible(visitedPOI < totalPOI);
    }

    private void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}