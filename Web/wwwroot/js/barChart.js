HandleBarChart = (canvasId, labels, data) => {
    const ctx = document.getElementById(canvasId);

    // Mapeo de producto a color
    const colorMap = {
        'Premium': { border: '#FF6787', background: '#FFB1C1' },
        'Magna': { border: '#53C2C2', background: '#A5DFDF' },
        'Diesel': { border: '#FFA953', background: '#FFCF9F' }
    };

    function getColor(label) {
        if (label.includes('Premium')) return colorMap['Premium'];
        if (label.includes('Magna')) return colorMap['Magna'];
        if (label.includes('Diesel')) return colorMap['Diesel'];
        const borderColor = stringToColor(label);
        return {
            border: borderColor,
            background: borderColor.replace(')', ', 0.5)').replace('hsl', 'hsla')
        }
    }

    function stringToColor(str) {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        const color = `hsl(${hash % 360}, 70%, 60%)`;
        return color;
    }

    const datasets = data.map((value, index) => {
        const color = getColor(labels[index]);
        return {
            label: labels[index] || `Combustible ${index + 1}`,
            data: [value],
            borderColor: color.border,
            backgroundColor: color.background,
            borderWidth: 2,
            borderRadius: 5,
            borderSkipped: false,
        };
    });

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Tanque'],
            datasets: datasets
        },
        options: {
            animation: {
                duration: 1500,
                easing: 'easeOutQuart',
                delay: (context) => context.datasetIndex * 200
            },
            responsive: true,
            responsiveAnimationDuration: 0,
            transitions: {
                active: {
                    animation: {
                        duration: 1500
                    }
                }
            }
        },
    });
}