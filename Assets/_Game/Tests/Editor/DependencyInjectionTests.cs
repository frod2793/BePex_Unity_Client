using NUnit.Framework;
using UnityEngine;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Models;
using BePex.EventSystem.Factories;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.ViewModelsDebug;
using BePex.EventSystem.Infrastructure;

namespace BePex.EventSystem.Tests
{
    /// <summary>
    /// [기능]: 외부 DI 프레임워크 없이 순수 DI(Pure DI) 방식으로 객체들이 엮일 때 순환 참조나 누락이 없는지 검증하는 테스트 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class DependencyInjectionTests
    {
        #region 유닛 테스트 메서드
        /// <summary>
        /// [기능]: EventSceneInitializer의 DI 초기화 과정을 시뮬레이션하여 예외가 발생하지 않는지 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: EventDebugViewModel 생성자에 CurrencyHUDViewModel 의존성 주입 반영
        /// </summary>
        [Test]
        public void Test_PureDI_Initialization_DryRun()
        {
            Assert.DoesNotThrow(() =>
            {
                // 1. 임시 세이브 시스템 (InMemory) 및 시간 제공자
                var saveSystem = new InMemorySaveSystem();
                var timeProvider = new SystemTimeProvider();

                // 2. 가상의 EventTable DTO
                var tableDTO = new EventTableDTO();

                // 3. 팩토리 생성
                var condFactory = new ConditionFactory(saveSystem, timeProvider);
                var rewFactory = new RewardFactory();

                // 4. 모델 생성
                var eventModel = new EventModel(tableDTO, condFactory, rewFactory, timeProvider);

                // 5. 뷰모델들 생성 (이 과정에서 누락된 매개변수나 예외가 발생하면 실패)
                var listVM = new EventListViewModel(eventModel, saveSystem);
                var playerReward = new PlayerRewardModel();
                var detailVM = new EventDetailViewModel(eventModel, saveSystem, playerReward);
                var popupVM = new RewardPopupViewModel(playerReward, saveSystem, eventModel);
                var hudVM = new CurrencyHUDViewModel(playerReward);
                var debugVM = new EventDebugViewModel(eventModel, saveSystem, timeProvider, playerReward, hudVM);

                // 리소스 정리 (IDisposable 구현 확인 및 메모리 해제)
                listVM.Dispose();
                detailVM.Dispose();
            }, "Pure DI 조립 과정에서 예외가 발생했습니다. 생성자 주입의 의존성이 깨졌을 수 있습니다.");
        }
        #endregion
    }
}
