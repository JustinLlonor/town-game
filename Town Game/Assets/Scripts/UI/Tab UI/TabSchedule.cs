using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Fusion;
using Pinwheel.Poseidon;

public class TabSchedule : MonoBehaviour
{
    public Transform blockHolder;
    public GameObject blockPrefab;
    public TextMeshProUGUI dateText;
    public int readDay;
    public Color primaryColor;
    public Color secondaryColor;
    PlayerRef selectedPlayer;
    ScheduleManager sm;
    GameManager gm;
    public float hourHeight = 0f;
    public float maxWidth = 262.7293f;

    struct BlockVolume
    {
        public float time;
        public float length;
        public List<ScheduleBlock> blocks;

        public BlockVolume(float time, float length, List<ScheduleBlock> blocks)
        {
            this.time = time;
            this.length = length;
            this.blocks = blocks;
        }
    }

    struct BlockBound
    {
        public float time;
        public bool isStart;
        public int blockIndex;

        public BlockBound(float time, bool isStart, int blockIndex)
        {
            this.time = time;
            this.isStart = isStart;
            this.blockIndex = blockIndex;
        }
    }

    private void Awake()
    {
        sm = FindFirstObjectByType<ScheduleManager>();
        gm = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        List<ScheduleBlock> sortedBlocks = sm.dailyBlocks.OrderBy(o => o.time).ToList();
        ScheduleBlock lastBlock = sortedBlocks[sm.dailyBlocks.Count - 1];
        hourHeight = blockHolder.GetComponent<RectTransform>().sizeDelta.y / ((lastBlock.time + lastBlock.length) - (sortedBlocks[0].time));
    }

    private void OnEnable()
    {
        DeselectSchedule(PlayerRef.None);
    }

    private void OnDisable()
    {
        selectedPlayer = PlayerRef.None;
    }

    // Shows the schedule a day in advance
    public void ScrollForward()
    {
        readDay++;
        if (selectedPlayer != PlayerRef.None) DisplaySchedule(selectedPlayer);
    }

    // Shows the schedule a day before
    public void ScrollBackward()
    {
        if (readDay == 0) return;
        readDay--;
        if (selectedPlayer != PlayerRef.None) DisplaySchedule(selectedPlayer);
    }

    public void DisplaySchedule(PlayerRef player)
    {
        selectedPlayer = player;
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = readDay * 24 - 1;
        float maxRange = readDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.dailyBlocks)
        {
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time + (gm.currentDay * 24));
            blocks.Add(block);
        }

        // Add mutable blocks from selected player
        foreach (ScheduleBlock block in sm.proxySchedules[player])
        {
            if (block.time < minRange || block.time > maxRange) continue;
            blocks.Add(block);
        }

        /**
        // Sort blocks
        blocks = blocks.OrderBy(o => o.time).ToList();

        // Add empty spaces
        List<ScheduleBlock> blocksCheck = new List<ScheduleBlock>(blocks);
        for (int i = 0; i < blocksCheck.Count; i++)
        {
            int nextI = i + 1;
            if (nextI >= blocksCheck.Count) break;
            if (blocksCheck[i].time + blocksCheck[i].length == blocksCheck[nextI].time) continue; // Continue if there is no space in between blocks
            blocks.Add(new ScheduleBlock("Free Time", "", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length));
        }
        **/

        // Create block volumes
        List<BlockBound> blockBounds = new List<BlockBound>();
        for (int i = 0; i < blocks.Count; i++)
        {
            blockBounds.Add(new BlockBound(blocks[i].time, true, i));
            blockBounds.Add(new BlockBound(blocks[i].time + blocks[i].length, false, i));
        }

        // Sort the block bounds
        blockBounds = blockBounds.OrderBy(o => o.time).ToList();
        List<BlockVolume> blockVolumes = new List<BlockVolume>();

        List<ScheduleBlock> currentBlocks = new List<ScheduleBlock>();
        float previousBound = -1f;
        foreach (BlockBound bound in blockBounds) // Create the volumes, every time there is a bound start it adds the block to a future volume, else it removes the block from future volume.
        {
            if (currentBlocks.Count > 0) blockVolumes.Add(new BlockVolume(previousBound, bound.time - previousBound, new List<ScheduleBlock>(currentBlocks)));
            if (bound.isStart) currentBlocks.Add(blocks[bound.blockIndex]);
            else currentBlocks.Remove(blocks[bound.blockIndex]);

            previousBound = bound.time;
        }

        UpdateSchedule(blockVolumes);

        // Change day text
        dateText.text = gm.GetDay(readDay);
    }

    // Sets the read day to the current day
    public void ResetReadDay(PlayerRef player)
    {
        readDay = gm.currentDay;
    }

    public void DeselectSchedule(PlayerRef player)
    {
        ClearSchedule();
        selectedPlayer = PlayerRef.None;
        dateText.text = "...";
    }

    void UpdateSchedule(List<BlockVolume> volumes)
    {
        // Clears blocks
        ClearSchedule();
        Debug.Log(volumes.Count);

        // Instantiates new blocks with color
        foreach (BlockVolume volume in volumes)
        {
            // Creates block
            float ySize = volume.length * hourHeight;
            float roundedTime = volume.time - 24f * gm.currentDay;
            float yPos = (roundedTime - 7f) * hourHeight;

            int i = 0;
            foreach (ScheduleBlock block in volume.blocks)
            {
                GameObject newBlock = Instantiate(blockPrefab, blockHolder);
                RectTransform bt = newBlock.GetComponent<RectTransform>();
                UIBlockPhys ubp = newBlock.GetComponent<UIBlockPhys>();
                float blockWidth = maxWidth / volume.blocks.Count;
                bt.sizeDelta = new Vector3(blockWidth, volume.length * hourHeight);
                bt.localPosition = new Vector3(blockWidth * i, -yPos);
                ubp.SetBlockColor(block.color);
                ubp.SetNameText(block.periodName);
                ubp.SetRoomText(block.room);
                i++;
            }
            //ubp.SetTimeText($"{gm.PeriodToClockString(block.time)} - {gm.PeriodToClockString(block.time + block.length)}");

            /**
            Image nbI = newBlock.GetComponent<Image>();

            // Sets color
            if (i % 2 == 0)
            {
                nbI.color = primaryColor;
            } else
            {
                nbI.color = secondaryColor;
            }
            i++;
            **/
        }
    }

    void ClearSchedule()
    {
        foreach (Transform child in blockHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
