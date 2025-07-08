using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarNode : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public float revealTime = 3f;
    public float revealTimer = 3f;
    public bool statRevealing = true;
    public AlphaGroup alphaGroup;
    public PhysNode physNode;
    public PlayerNodes trackedNodes;
    public int trackedNodeId = -1;
    private bool init = false;
    private bool startAnimationPlayed = false;

    public void Init(int nodeId, PlayerNodes trackedPlayerNodes)
    {
        trackedNodeId = nodeId;
        trackedNodes = trackedPlayerNodes;
        Node currentNode = trackedNodes.GetNode(nodeId);
        NodeInfo info = trackedNodes.GetNodeInfo(currentNode.infoIndex);
        physNode.Init(info);
        init = true;
    }

    public void ResetTime()
    {
        if (revealTimer > revealTime - fadeTime) return;
        revealTimer = revealTime - fadeTime;
    }

    private void Update()
    {
        if (!init) return;
        if (revealTimer <= 0f)
        {
            statRevealing = false;
            return;
        }
        if (trackedNodes == null) return;
        Node currentNode = trackedNodes.GetNode(trackedNodeId);
        physNode.SetStatusText(currentNode.value);
        PlayAlphaAnimation();
        revealTimer -= Time.deltaTime;
    }

    private void PlayAlphaAnimation()
    {
        if ((revealTimer >= (revealTime - fadeTime)) && (revealTimer <= revealTime))
        {
            if (!startAnimationPlayed) ProcessStartAnim();
            return;
        }
        startAnimationPlayed = true;
        if ((revealTimer >= 0f) && (revealTimer <= fadeTime))
        {
            ProcessEndAnim();
            return;
        }
        if (alphaGroup.alpha != 1f)
        {
            alphaGroup.SetAlpha(1f);
        }
    }

    private void ProcessStartAnim()
    {
        float progress = 1f - ((revealTimer - (revealTime - fadeTime)) * (1/fadeTime));
        alphaGroup.SetAlpha(Mathf.Lerp(0f, 1f, progress));
    }

    private void ProcessEndAnim()
    {
        float progress = revealTimer * (1/fadeTime);
        alphaGroup.SetAlpha(Mathf.Lerp(0f, 1f, progress));
    }
}
