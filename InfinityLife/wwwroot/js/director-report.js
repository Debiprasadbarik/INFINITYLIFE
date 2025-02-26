document.addEventListener('DOMContentLoaded', function () {
    // Calculate summary statistics
    calculateSummaryStats();

    // Initialize event listeners
    initializeEventListeners();
});

function calculateSummaryStats() {
    const timesheetRows = document.querySelectorAll('tbody tr');

    let totalTimesheets = timesheetRows.length;
    let totalHours = 0;
    let totalProjectHours = 0;
    let totalLearningHours = 0;
    let pendingTimesheets = 0;

    timesheetRows.forEach(row => {
        const totalHoursCell = row.cells[2].textContent.trim();
        const projectHoursCell = row.cells[3].textContent.trim();
        const learningHoursCell = row.cells[4].textContent.trim();
        const statusCell = row.cells[5].textContent.trim();

        totalHours += parseFloat(totalHoursCell) || 0;
        totalProjectHours += parseFloat(projectHoursCell) || 0;
        totalLearningHours += parseFloat(learningHoursCell) || 0;

        if (statusCell.includes('Pending')) {
            pendingTimesheets++;
        }
    });

    // Create summary cards
    const summaryStats = document.getElementById('summaryStats');
    summaryStats.innerHTML = `
        <div class="bg-blue-100 p-4 rounded-lg">
            <h3 class="font-semibold text-blue-800">Total Timesheets</h3>
            <p class="text-2xl font-bold">${totalTimesheets}</p>
            <p class="text-sm text-blue-600">${pendingTimesheets} pending approval</p>
        </div>
        <div class="bg-green-100 p-4 rounded-lg">
            <h3 class="font-semibold text-green-800">Total Hours</h3>
            <p class="text-2xl font-bold">${totalHours.toFixed(1)}</p>
            <p class="text-sm text-green-600">Across all timesheets</p>
        </div>
        <div class="bg-purple-100 p-4 rounded-lg">
            <h3 class="font-semibold text-purple-800">Hours Distribution</h3>
            <p class="text-md">Project: ${totalProjectHours.toFixed(1)} (${((totalProjectHours / totalHours) * 100).toFixed(1)}%)</p>
            <p class="text-md">Learning: ${totalLearningHours.toFixed(1)} (${((totalLearningHours / totalHours) * 100).toFixed(1)}%)</p>
        </div>
    `;
}

function initializeEventListeners() {
    // View details buttons
    document.querySelectorAll('.view-details').forEach(button => {
        button.addEventListener('click', function () {
            const timesheetId = this.getAttribute('data-timesheet-id');
            showTimesheetDetails(timesheetId);
        });
    });

    // Approve timesheet buttons
    document.querySelectorAll('.approve-timesheet').forEach(button => {
        button.addEventListener('click', function () {
            const timesheetId = this.getAttribute('data-timesheet-id');
            if (confirm('Are you sure you want to approve this timesheet?')) {
                updateTimesheetStatus(timesheetId, 'Approved');
            }
        });
    });

    // Reject timesheet buttons
    document.querySelectorAll('.reject-timesheet').forEach(button => {
        button.addEventListener('click', function () {
            const timesheetId = this.getAttribute('data-timesheet-id');
            const reason = prompt('Please provide a reason for rejection:');
            if (reason) {
                updateTimesheetStatus(timesheetId, 'Rejected', reason);
            }
        });
    });

    // Modal close button
    document.getElementById('closeModal').addEventListener('click', function () {
        document.getElementById('timesheetDetailsModal').classList.add('hidden');
    });
}

async function showTimesheetDetails(timesheetId) {
    try {
        const response = await fetch(`/TimeSheet/Details/${timesheetId}`);
        if (!response.ok) throw new Error('Failed to load timesheet details');

        const detailsHtml = await response.text();
        document.getElementById('timesheetDetailsContent').innerHTML = detailsHtml;
        document.getElementById('timesheetDetailsModal').classList.remove('hidden');
    } catch (error) {
        console.error('Error loading timesheet details:', error);
        alert('Failed to load timesheet details. Please try again.');
    }
}

async function updateTimesheetStatus(timesheetId, status, reason = '') {
    try {
        const data = {
            timesheetId: timesheetId,
            status: status,
            reason: reason
        };

        const response = await fetch('/TimeSheet/UpdateStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) throw new Error('Failed to update timesheet status');

        // Reload the page to show updated status
        window.location.reload();
    } catch (error) {
        console.error('Error updating timesheet status:', error);
        alert('Failed to update timesheet status. Please try again.');
    }
}