using System.Linq;
using LevelGenerator.Scripts.Helpers;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class Section : MonoBehaviour
    {
        /// <summary>
        /// Section tags
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// Tags that this section can annex
        /// </summary>
        public string[] CreatesTags;

        /// <summary>
        /// Exits node in hierarchy
        /// </summary>
        public Exits Exits;

        /// <summary>
        /// Bounds node in hierarchy
        /// </summary>
        public Bounds Bounds;

        /// <summary>
        /// Chances of the section spawning a dead end
        /// </summary>
        public int DeadEndChance;

        protected LevelGenerator LevelGenerator;
        protected int order;
        
        public void Initialize(LevelGenerator levelGenerator, int sourceOrder)
        {
            LevelGenerator = levelGenerator;
            transform.SetParent(LevelGenerator.Container);
            LevelGenerator.RegisterNewSection(this);
            order = sourceOrder + 1;

            GenerateAnnexes();
        }

        protected void GenerateAnnexes()
        {
            if (CreatesTags.Any())
            {
                foreach (var e in Exits.ExitSpots)
                {
                    if (LevelGenerator.LevelSize > 0 && order < LevelGenerator.MaxAllowedOrder)
                        if (RandomService.RollD100(DeadEndChance))
                            PlaceDeadEnd(e);
                        else
                            GenerateSection(e);
                    else
                        PlaceDeadEnd(e);
                }
            }
        }

        protected void GenerateSection(Transform exit)
        {
            var candidate = IsAdvancedExit(exit)
                ? BuildSectionFromExit(exit.GetComponent<AdvancedExit>())
                : BuildSectionFromExit(exit);
                
            if (LevelGenerator.IsSectionValid(candidate.Bounds, Bounds))
            {
                candidate.Initialize(LevelGenerator, order);
            }
            else
            {
                Destroy(candidate.gameObject);
                PlaceDeadEnd(exit);
            }
        }

        protected void PlaceDeadEnd(Transform exit) => Instantiate(LevelGenerator.DeadEnds.PickOne(), exit).Initialize(LevelGenerator);

        protected bool IsAdvancedExit(Transform exit) => exit.GetComponent<AdvancedExit>() != null;

        protected Section BuildSectionFromExit(Transform exit) => Instantiate(LevelGenerator.PickSectionWithTag(CreatesTags), exit).GetComponent<Section>();

        protected Section BuildSectionFromExit(AdvancedExit exit) => Instantiate(LevelGenerator.PickSectionWithTag(exit.CreatesTags), exit.transform).GetComponent<Section>();
    }
}