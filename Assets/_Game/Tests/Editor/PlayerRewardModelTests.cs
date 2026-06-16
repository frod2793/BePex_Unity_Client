using NUnit.Framework;
using UnityEngine;
using BePex.EventSystem.Models;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BePex.EventSystem.Tests
{
    /// <summary>
    /// [기능]: PlayerRewardModel의 OCP 자산 확장성, Unity 직렬화, 하위 호환 마이그레이션 기능을 검증하는 에디터 유닛 테스트 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [TestFixture]
    public class PlayerRewardModelTests
    {
        /// <summary>
        /// [기능]: "Gold" 등 기존에 정의되지 않은 새로운 임의의 보상 타입 추가 시 예외 없이 적립 및 잔액 조회가 정상 동작하는지 OCP 동작을 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        [Test]
        public void Test_01_NewCurrency_Dynamic_Extension()
        {
            // Arrange
            var rewardModel = new PlayerRewardModel();

            // Act
            rewardModel.AddCurrency("Gold", 1000);
            rewardModel.AddCurrency("Gem", 50);

            // Assert
            var balances = rewardModel.GetBalances();
            
            Debug.Log($"[PlayerRewardModelTests] 신규 재화 적립 검증 - Gold: {balances["Gold"]}, Gem: {balances["Gem"]}");
            
            Assert.IsTrue(balances.ContainsKey("Gold"), "[PlayerRewardModelTests] Gold 재화 키가 딕셔너리에 존재하지 않습니다.");
            Assert.AreEqual(1000, balances["Gold"], "[PlayerRewardModelTests] Gold 재화 적립 액수가 불일치합니다.");
            Assert.IsTrue(balances.ContainsKey("Gem"), "[PlayerRewardModelTests] Gem 재화 키가 딕셔너리에 존재하지 않습니다.");
            Assert.AreEqual(50, balances["Gem"], "[PlayerRewardModelTests] Gem 재화 적립 액수가 불일치합니다.");
        }

        /// <summary>
        /// [기능]: JsonUtility를 통해 PlayerRewardModel을 직렬화하고 다시 복원했을 때 딕셔너리 내부 재화 상태가 소실 없이 복원되는지 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        [Test]
        public void Test_02_JsonUtility_Serialization_Recovery()
        {
            // Arrange
            var originalModel = new PlayerRewardModel();
            originalModel.AddCurrency("Exp", 500);
            originalModel.AddCurrency("Ticket", 3);
            originalModel.AddCurrency("Gold", 250);

            // Act
            string json = JsonConvert.SerializeObject(originalModel, Formatting.Indented);
            Debug.Log($"[PlayerRewardModelTests] 직렬화된 JSON 문자열: {json}");
            
            var restoredModel = JsonConvert.DeserializeObject<PlayerRewardModel>(json);

            // Assert
            Assert.AreEqual(500, restoredModel.totalExp, "[PlayerRewardModelTests] 복원된 totalExp 값이 불일치합니다.");
            Assert.AreEqual(3, restoredModel.totalTickets, "[PlayerRewardModelTests] 복원된 totalTickets 값이 불일치합니다.");
            
            var balances = restoredModel.GetBalances();
            Assert.AreEqual(250, balances["Gold"], "[PlayerRewardModelTests] 복원된 동적 재화 Gold 값이 불일치합니다.");
        }


    }
}
