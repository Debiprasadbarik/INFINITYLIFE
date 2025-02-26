// Timesheet form handling
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('timesheetForm');
    const addEntryBtn = document.getElementById('addEntry');
    const entriesContainer = document.getElementById('entriesContainer');
    const entryTemplate = document.getElementById('entryTemplate');
    let entryCount = 0;

    // Initialize with one entry
    addEntry();

    // Add new entry
    addEntryBtn.addEventListener('click', addEntry);

    function addEntry() {
        const entryHtml = entryTemplate.innerHTML.replace(/{index}/g, entryCount);
        const entryWrapper = document.createElement('div');
        entryWrapper.innerHTML = entryHtml;
        entriesContainer.appendChild(entryWrapper.firstElementChild);

        // Set up event listeners for the new entry
        const newEntry = entriesContainer.lastElementChild;
        setupEntryEventListeners(newEntry);

        entryCount++;
        updateTotalHours();
    }

    function setupEntryEventListeners(entry) {
        // Type change handler
        const typeSelect = entry.querySelector('.entry-type');
        typeSelect.addEventListener('change', function () {
            const projectFields = entry.querySelector('.project-fields');
            const learningFields = entry.querySelector('.learning-fields');

            if (this.value === 'Project') {
                projectFields.classList.remove('hidden');
                learningFields.classList.add('hidden');
                learningFields.querySelector('input').removeAttribute('required');
                projectFields.querySelector('input').setAttribute('required', 'required');
            } else if (this.value === 'Learning') {
                learningFields.classList.remove('hidden');
                projectFields.classList.add('hidden');
                projectFields.querySelector('input').removeAttribute('required');
                learningFields.querySelector('input').setAttribute('required', 'required');
            } else {
                projectFields.classList.add('hidden');
                learningFields.classList.add('hidden');
                projectFields.querySelector('input').removeAttribute('required');
                learningFields.querySelector('input').removeAttribute('required');
            }
        });

        // Hours change handler
        const hoursInput = entry.querySelector('.entry-hours');
        hoursInput.addEventListener('change', updateTotalHours);

        // Remove entry handler
        const removeBtn = entry.querySelector('.remove-entry');
        removeBtn.addEventListener('click', function () {
            if (entriesContainer.children.length > 1) {
                entry.remove();
                updateTotalHours();
            } else {
                alert('At least one entry is required');
            }
        });

        // Date validation
        const dateInput = entry.querySelector('.entry-date');
        dateInput.addEventListener('change', function () {
            validateDateRange(dateInput);
        });
    }

    function updateTotalHours() {
        const hourInputs = document.querySelectorAll('.entry-hours');
        const total = Array.from(hourInputs)
            .reduce((sum, input) => sum + (parseFloat(input.value) || 0), 0);

        document.getElementById('totalHours').textContent = total.toFixed(1);

        // Update hidden total hours field
        const totalHoursInput = document.querySelector('input[name="TimeSheet.TotalHours"]');
        if (!totalHoursInput) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'TimeSheet.TotalHours';
            form.appendChild(input);
        }
        totalHoursInput.value = total;
    }

    function validateDateRange(dateInput) {
        const startDate = new Date(document.querySelector('input[name="TimeSheet.StartDate"]').value);
        const endDate = new Date(document.querySelector('input[name="TimeSheet.EndDate"]').value);
        const entryDate = new Date(dateInput.value);

        if (entryDate < startDate || entryDate > endDate) {
            dateInput.setCustomValidity('Entry date must be within the timesheet date range');
        } else {
            dateInput.setCustomValidity('');
        }
    }

    // Form submission handler
    form.addEventListener('submit', function (e) {
        const totalHours = parseFloat(document.getElementById('totalHours').textContent);

        if (totalHours < 45) {
            e.preventDefault();
            alert('Total hours must be at least 45');
            return;
        }

        // Validate all dates are within range
        const dateInputs = document.querySelectorAll('.entry-date');
        dateInputs.forEach(validateDateRange);

        if (!form.checkValidity()) {
            e.preventDefault();
            alert('Please fill in all required fields correctly');
        }
    });
});