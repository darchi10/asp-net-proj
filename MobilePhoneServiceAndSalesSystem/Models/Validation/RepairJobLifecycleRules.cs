using MobilePhoneServiceAndSalesSystem.Models.Enums;

namespace MobilePhoneServiceAndSalesSystem.Models.Validation;

public sealed record RepairJobValidationError(string Key, string Message);

public static class RepairJobLifecycleRules
{
    public static IReadOnlyList<RepairJobValidationError> ValidateSnapshot(
        RepairStatus status,
        DateTime receivedDate,
        DateTime? completedDate,
        DateTime now)
    {
        var errors = new List<RepairJobValidationError>();

        if (!Enum.IsDefined(typeof(RepairStatus), status))
        {
            errors.Add(new RepairJobValidationError("Status", "Choose a valid repair status."));
            return errors;
        }

        if (receivedDate == default)
        {
            errors.Add(new RepairJobValidationError("ReceivedDate", "Received date is required."));
        }
        else if (receivedDate > now)
        {
            errors.Add(new RepairJobValidationError("ReceivedDate", "Received date cannot be in the future."));
        }

        var requiresCompletionDate = status is RepairStatus.Completed or RepairStatus.Delivered;
        if (requiresCompletionDate && !completedDate.HasValue)
        {
            errors.Add(new RepairJobValidationError("CompletedDate", "Completed date is required when a repair is completed or delivered."));
        }

        if (!requiresCompletionDate && completedDate.HasValue)
        {
            errors.Add(new RepairJobValidationError("CompletedDate", "Completed date can only be set for completed or delivered repairs."));
        }

        if (completedDate.HasValue)
        {
            if (completedDate.Value > now)
            {
                errors.Add(new RepairJobValidationError("CompletedDate", "Completed date cannot be in the future."));
            }

            if (receivedDate != default && completedDate.Value < receivedDate)
            {
                errors.Add(new RepairJobValidationError("CompletedDate", "Completed date cannot be earlier than received date."));
            }
        }

        return errors;
    }

    public static IReadOnlyList<RepairJobValidationError> ValidateUpdate(
        RepairStatus currentStatus,
        RepairStatus requestedStatus,
        DateTime receivedDate,
        DateTime? completedDate,
        DateTime now)
    {
        var errors = ValidateSnapshot(requestedStatus, receivedDate, completedDate, now).ToList();

        if (Enum.IsDefined(typeof(RepairStatus), currentStatus)
            && Enum.IsDefined(typeof(RepairStatus), requestedStatus)
            && !IsTransitionAllowed(currentStatus, requestedStatus))
        {
            errors.Add(new RepairJobValidationError(
                "Status",
                $"Status cannot change from {currentStatus} to {requestedStatus}."));
        }

        return errors;
    }

    public static bool IsTransitionAllowed(RepairStatus currentStatus, RepairStatus requestedStatus)
    {
        if (currentStatus == requestedStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            RepairStatus.Pending => requestedStatus is RepairStatus.InProgress or RepairStatus.Cancelled,
            RepairStatus.InProgress => requestedStatus is RepairStatus.Completed or RepairStatus.Cancelled,
            RepairStatus.Completed => requestedStatus == RepairStatus.Delivered,
            RepairStatus.Delivered or RepairStatus.Cancelled => false,
            _ => false
        };
    }
}
