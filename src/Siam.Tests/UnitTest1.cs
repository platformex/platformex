using Orleans.TestKit;
using Platformex.Application;
using Platformex.Domain;
using Platformex.Tests;
using Siam.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;
using System;
using Xunit;

namespace Siam.Tests
{
    public class UnitTest1 : PlatformexTestKit
    {
        public UnitTest1()
        {
            Silo.AddService<IMemoState>(new MemoState(new InMemoryDbProvider<MemoModel>()));
        }

        [Fact]
        public void UpdateMemoTest()
        {
            //������� ��������� ��� �����
            var fixture = new AggregateFixture<MemoId, MemoAggregate, IMemoState, MemoState>(this);

            var id = MemoId.New;

            //��������� ���������
            var docId = Guid.NewGuid().ToString();
            var docNumber = new DocumentNumber("100");
            var docAddress = new Address("12700", "������",
                "������", "�������� ����", "1");

            //BDD ���� (��������)
            fixture.For(id)

                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .When(new UpdateMemo(id,
                    new MemoDocument(docId, docNumber, docAddress)))

                //����� (�������� �����������)
                .ThenExpectResult(e => e.IsSuccess)
                .ThenExpectDomainEvent<MemoUpdated>(e
                    => e.AggregateEvent.Id == id
                       && e.AggregateEvent.Document != null
                       && e.AggregateEvent.Document.Id == docId
                       && e.AggregateEvent.Document.Number == docNumber
                       && e.AggregateEvent.Document.CustomerAddress == docAddress)
                .ThenExpectState(s
                    => s.Status == MemoStatus.Undefined
                       && s.Document != null
                       && s.Document.Id == docId
                       && s.Document.Number == docNumber
                       && s.Document.CustomerAddress == docAddress);
        }
        [Fact]
        public void TestSaga()
        {
            var id = MemoId.New;
            var fixture = new SagaFixture<AutoConfimMemoSaga, AutoConfirmSagaState>(this);

            //BDD ���� (��������)
            fixture.For()

                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .When<MemoId, SigningStarted>(new SigningStarted(id, "TestUser"))

                //����� (�������� �����������)
                .ThenExpect<MemoId, ConfirmSigningMemo>(command => command.Id == id)

                //����� (����������� ��������)
                .AndWhen<MemoId, MemoSigned>(new MemoSigned(id))

                //����� (�������� �����������)
                .ThenExpectState(state => state.UserId == "TestUser");
        }

        [Fact]
        public void TestService()
        {
            var fixture = new ServiceFixture<IMemoService, MemoService>(this);

            //BDD ���� (��������)
            fixture.For()

                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .When(async service => await service.CreateMemos(2))

                //����� (�������� �����������)
                .ThenExpect<MemoId, UpdateMemo>(command => command.Document != null)
                .ThenExpect<MemoId, UpdateMemo>(command => command.Document != null);
        }

        [Fact]
        public void TestSubscriber()
        {
            var id = MemoId.New;

            var docId = Guid.NewGuid().ToString();
            var docNumber = new DocumentNumber("100");
            var docAddress = new Address("12700", "������",
                "������", "�������� ����", "1");
            var userId = "TestUser";

            var fixture = new SubscriberFixture<MemoSigningSubscriber, MemoId, MemoUpdated>(this);

            //BDD ���� (��������)
            fixture.For()

                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .When(new MemoUpdated(id, new MemoDocument(docId, docNumber, docAddress)),
                    new EventMetadata { UserId = userId })

                //����� (�������� �����������)
                .ThenExpect<MemoId, SignMemo>(command =>
                    command.Id == id
                    && command.UserId == userId);
        }


        [Fact]
        public void TestJob()
        {
            var fixture = new JobFixture<MemoJob>(this);

            //BDD ���� (��������)
            fixture.For()

                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .WhenTimer()

                //����� (�������� �����������)
                .ThenExpect<MemoId, UpdateMemo>(command =>
                    command.Document.CustomerAddress.City == "������");
        }
    }
}