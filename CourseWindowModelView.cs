using System;
using System.Collections.Generic;

namespace duoo
{

    public enum CourseType
    {
        Free,
        Premium
    }

    public class Course
    {
        public string Title { get; set; }
        public CourseType Type { get; set; }
        public int Price { get; set; } = 0;
        public string Difficulty { get; set; } = "Unknown";
        public List<string> Topics { get; set; } = new();
        public List<Module> Modules { get; set; } = new();
public Module? BonusModule { get; set; } = null;

        public bool IsCompleted()
        {
            return Modules.All(m => m.IsCompleted);
        }
    }
    public class CourseWindowModuleView
    {
        private readonly Course _course;
        private readonly List<ModuleViewModel> _moduleViewModels = new();

        public CourseWindowModuleView(Course course)
        {
            _course = course;
            GenerateModuleViewModels();
        }

        private void GenerateModuleViewModels()
        {
            _moduleViewModels.Clear();

            for (int i = 0; i < _course.Modules.Count; i++)
            {
                bool isUnlocked = i == 0 || _course.Modules[i - 1].IsCompleted;
                _moduleViewModels.Add(new ModuleViewModel(_course.Modules[i], i, isUnlocked));
            }

            if (_course.BonusModule != null)
            {
                bool isBonusUnlocked = _course.BonusModule.BonusUnlockCost == 0;
                _moduleViewModels.Add(new ModuleViewModel(_course.BonusModule, _moduleViewModels.Count, isBonusUnlocked));
            }
        }

        public void DisplayModules()
        {
            Console.WriteLine($"\nCourse: {_course.Title} | Difficulty: {_course.Difficulty}\nModules:");

            foreach (var vm in _moduleViewModels)
            {
                Console.WriteLine(vm.GetDisplayString());
            }
        }

        public void CompleteModule(int index)
        {
            if (index >= 0 && index < _moduleViewModels.Count)
            {
                var vm = _moduleViewModels[index];
                if (vm.CanBeCompleted())
                {
                    vm.Module.MarkCompleted();
                    Console.WriteLine($"Module {index + 1} marked as completed.");
                    GenerateModuleViewModels(); // refresh unlocks
                }
                else
                {
                    Console.WriteLine("Module is locked or already completed.");
                }
            }
        }

        public void ClickImageReward(int index)
        {
            if (index >= 0 && index < _moduleViewModels.Count)
            {
                var vm = _moduleViewModels[index];
                if (vm.CanCollectReward())
                {
                    int reward = vm.Module.ClickImage();
                    Console.WriteLine($"Collected {reward} coins from image in Module {index + 1}.");
                }
                else
                {
                    Console.WriteLine("No reward available or already collected.");
                }
            }
        }

        public void UnlockBonusModule()
        {
            if (_course.BonusModule != null)
            {
                _course.BonusModule.BonusUnlockCost = 0; // simulate unlock
                Console.WriteLine("Bonus module unlocked.");
                GenerateModuleViewModels();
            }
        }
    }
}