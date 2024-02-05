using Pulse.Timeline.Services;

namespace Pulse.Timeline.Tests;

public class TimelineServiceTests
{
    public class Given_NewUser
    {
        [Fact]
        public async Task GetTimelinePage_ReturnsEmpty()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(Guid.NewGuid(), null, 10);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsEmpty_WhenCursorIsNotNull()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(Guid.NewGuid(), "cursor", 10);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsEmpty_WhenCountIsZero()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(Guid.NewGuid(), null, 0);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsEmpty_WhenUserIdIsEmpty()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(Guid.Empty, null, 10);
            Assert.Empty(result);
        }
    }

    public class Given_ExistingUser
    {
        private readonly Guid _userId;
        private readonly List<Guid> _posts;

        public Given_ExistingUser()
        {
            _userId = Guid.NewGuid();
            _posts = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsPosts()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(_userId, null, 10);
            Assert.Equal(_posts, result);
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsPosts_WhenCursorIsNotNull()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(_userId, null, 5);
            var secondPage = await service.GetTimelinePage(_userId, result.Last().ToString(), 5);
            Assert.Equal(_posts.Skip(5).Take(5), result);
        }

        [Fact]
        public async Task GetTimelinePage_ReturnsEmpty_WhenCountIsZero()
        {
            var service = new TimelineService();
            var result = await service.GetTimelinePage(_userId, null, 0);
            Assert.Empty(result);
        }
    }
}
