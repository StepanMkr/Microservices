using MassTransit;
using Microsoft.Extensions.Logging;
using SagaContracts.Choreography;
using Services.Contracts.Dtos.Creations;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Services.Messages.Consumers.Choreography
{
    public class ProjectDeletedConsumer : IConsumer<ProjectDeleted>
    {
        private readonly ICreationsService _creationsService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProjectDeletedConsumer> _logger;

        public ProjectDeletedConsumer(
            ICreationsService creationsService,
            IPublishEndpoint publishEndpoint,
            ILogger<ProjectDeletedConsumer> logger)
        {
            _creationsService = creationsService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProjectDeleted> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Choreography: ProjectDeleted received: ProjectId={ProjectId}, CreationId={CreationId}",
                message.ProjectId, message.CreationId);

            var confirmRequest = new CreationConfirmRequest
            {
                UserId = Guid.Empty // TODO: Replace with actual UserId if available
            };

            try
            {
                await _creationsService.ConfirmAsync(message.CreationId, confirmRequest);

                await _publishEndpoint.Publish<CreationConfirmed>(new
                {
                    message.CreationId,
                    message.ProjectId
                });

                _logger.LogInformation(
                    "Choreography: CreationConfirmed published: CreationId={CreationId}",
                    message.CreationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing ProjectDeleted for CreationId={CreationId}, ProjectId={ProjectId}",
                    message.CreationId, message.ProjectId);
                throw;
            }
        }
    }
}
