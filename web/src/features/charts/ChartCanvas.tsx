import React, { useMemo } from 'react';

type WellKey = 'axis' | 'legend' | 'values' | 'filters';

type ChartWells = Record<WellKey, string[]>;

interface ChartCanvasProps {
  vizType: string;
  wells: ChartWells;
}

const PALETTE = [
  '#2563eb',
  '#16a34a',
  '#f59e0b',
  '#db2777',
  '#7c3aed',
  '#0891b2',
  '#dc2626',
  '#65a30d',
];

const hashString = (value: string): number => {
  let hash = 0;
  for (let index = 0; index < value.length; index += 1) {
    hash = (hash * 31 + value.charCodeAt(index)) | 0;
  }
  return Math.abs(hash);
};

const seededRandom = (seed: number): (() => number) => {
  let state = seed % 2147483647;
  if (state <= 0) state += 2147483646;
  return () => {
    state = (state * 16807) % 2147483647;
    return (state - 1) / 2147483646;
  };
};

interface SampleData {
  categories: string[];
  measures: string[];
  values: number[][]; // values[categoryIndex][measureIndex]
}

const buildSampleData = (wells: ChartWells): SampleData => {
  const dimensionField = wells.axis[0] ?? wells.legend[0] ?? null;
  const measures = wells.values.length > 0 ? wells.values : ['Value'];

  const categoryCount = 6;
  const baseSeed = hashString(dimensionField ?? measures.join('|') ?? 'nexova');
  const rng = seededRandom(baseSeed + 7);

  const categories = Array.from({ length: categoryCount }, (_, index) =>
    dimensionField ? `${dimensionField} ${index + 1}` : `Item ${index + 1}`
  );

  const values = categories.map(() =>
    measures.map(() => Math.round(20 + rng() * 80))
  );

  return { categories, measures, values };
};

const niceMax = (max: number): number => {
  if (max <= 0) return 1;
  const magnitude = 10 ** Math.floor(Math.log10(max));
  const normalized = max / magnitude;
  const step = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
  return step * magnitude;
};

const BarChart: React.FC<{ data: SampleData }> = ({ data }) => {
  const width = 480;
  const height = 280;
  const padding = { top: 16, right: 16, bottom: 36, left: 40 };
  const plotW = width - padding.left - padding.right;
  const plotH = height - padding.top - padding.bottom;

  const max = niceMax(Math.max(...data.values.flat(), 1));
  const groupWidth = plotW / data.categories.length;
  const barWidth = (groupWidth * 0.7) / data.measures.length;

  return (
    <svg viewBox={`0 0 ${width} ${height}`} className="h-full w-full">
      {[0, 0.25, 0.5, 0.75, 1].map((tick) => {
        const y = padding.top + plotH * (1 - tick);
        return (
          <g key={tick}>
            <line x1={padding.left} y1={y} x2={width - padding.right} y2={y} stroke="#e5e7eb" />
            <text x={padding.left - 6} y={y + 3} textAnchor="end" fontSize="9" fill="#9ca3af">
              {Math.round(max * tick)}
            </text>
          </g>
        );
      })}
      {data.categories.map((category, ci) => {
        const groupX = padding.left + ci * groupWidth + groupWidth * 0.15;
        return (
          <g key={category}>
            {data.measures.map((measure, mi) => {
              const value = data.values[ci][mi];
              const barH = (value / max) * plotH;
              const x = groupX + mi * barWidth;
              const y = padding.top + plotH - barH;
              return (
                <rect
                  key={measure}
                  x={x}
                  y={y}
                  width={barWidth - 2}
                  height={barH}
                  rx={2}
                  fill={PALETTE[mi % PALETTE.length]}
                />
              );
            })}
            <text
              x={padding.left + ci * groupWidth + groupWidth / 2}
              y={height - padding.bottom + 14}
              textAnchor="middle"
              fontSize="9"
              fill="#6b7280"
            >
              {category.length > 10 ? `${category.slice(0, 9)}…` : category}
            </text>
          </g>
        );
      })}
    </svg>
  );
};

const LineChart: React.FC<{ data: SampleData; area?: boolean }> = ({ data, area }) => {
  const width = 480;
  const height = 280;
  const padding = { top: 16, right: 16, bottom: 36, left: 40 };
  const plotW = width - padding.left - padding.right;
  const plotH = height - padding.top - padding.bottom;
  const max = niceMax(Math.max(...data.values.flat(), 1));
  const stepX = data.categories.length > 1 ? plotW / (data.categories.length - 1) : 0;

  const pointFor = (ci: number, value: number) => ({
    x: padding.left + ci * stepX,
    y: padding.top + plotH - (value / max) * plotH,
  });

  return (
    <svg viewBox={`0 0 ${width} ${height}`} className="h-full w-full">
      {[0, 0.25, 0.5, 0.75, 1].map((tick) => {
        const y = padding.top + plotH * (1 - tick);
        return (
          <g key={tick}>
            <line x1={padding.left} y1={y} x2={width - padding.right} y2={y} stroke="#e5e7eb" />
            <text x={padding.left - 6} y={y + 3} textAnchor="end" fontSize="9" fill="#9ca3af">
              {Math.round(max * tick)}
            </text>
          </g>
        );
      })}
      {data.measures.map((measure, mi) => {
        const color = PALETTE[mi % PALETTE.length];
        const points = data.categories.map((_, ci) => pointFor(ci, data.values[ci][mi]));
        const linePath = points.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x},${p.y}`).join(' ');
        const areaPath =
          `M${points[0].x},${padding.top + plotH} ` +
          points.map((p) => `L${p.x},${p.y}`).join(' ') +
          ` L${points[points.length - 1].x},${padding.top + plotH} Z`;
        return (
          <g key={measure}>
            {area && <path d={areaPath} fill={color} opacity={0.15} />}
            <path d={linePath} fill="none" stroke={color} strokeWidth={2} />
            {points.map((p, i) => (
              <circle key={i} cx={p.x} cy={p.y} r={2.5} fill={color} />
            ))}
          </g>
        );
      })}
      {data.categories.map((category, ci) => (
        <text
          key={category}
          x={padding.left + ci * stepX}
          y={height - padding.bottom + 14}
          textAnchor="middle"
          fontSize="9"
          fill="#6b7280"
        >
          {category.length > 10 ? `${category.slice(0, 9)}…` : category}
        </text>
      ))}
    </svg>
  );
};

const PieChart: React.FC<{ data: SampleData }> = ({ data }) => {
  const size = 280;
  const radius = 110;
  const cx = size / 2;
  const cy = size / 2;
  const totals = data.categories.map((_, ci) => data.values[ci][0]);
  const sum = totals.reduce((acc, value) => acc + value, 0) || 1;

  let angle = -Math.PI / 2;
  const slices = totals.map((value, ci) => {
    const portion = value / sum;
    const start = angle;
    const end = angle + portion * Math.PI * 2;
    angle = end;
    const largeArc = end - start > Math.PI ? 1 : 0;
    const x1 = cx + radius * Math.cos(start);
    const y1 = cy + radius * Math.sin(start);
    const x2 = cx + radius * Math.cos(end);
    const y2 = cy + radius * Math.sin(end);
    return {
      path: `M${cx},${cy} L${x1},${y1} A${radius},${radius} 0 ${largeArc} 1 ${x2},${y2} Z`,
      color: PALETTE[ci % PALETTE.length],
      label: data.categories[ci],
      percent: Math.round(portion * 100),
    };
  });

  return (
    <div className="flex h-full w-full items-center justify-center gap-6">
      <svg viewBox={`0 0 ${size} ${size}`} className="h-full max-h-[260px]">
        {slices.map((slice) => (
          <path key={slice.label} d={slice.path} fill={slice.color} stroke="#fff" strokeWidth={1.5} />
        ))}
      </svg>
      <ul className="space-y-1 text-xs">
        {slices.map((slice) => (
          <li key={slice.label} className="flex items-center gap-2 text-gray-600">
            <span className="h-2.5 w-2.5 rounded-sm" style={{ backgroundColor: slice.color }} />
            <span className="truncate">{slice.label}</span>
            <span className="ml-auto font-medium text-gray-800">{slice.percent}%</span>
          </li>
        ))}
      </ul>
    </div>
  );
};

const TreemapChart: React.FC<{ data: SampleData }> = ({ data }) => {
  const totals = data.categories.map((_, ci) => data.values[ci][0]);
  const sum = totals.reduce((acc, value) => acc + value, 0) || 1;
  const ordered = totals
    .map((value, index) => ({ value, index }))
    .sort((a, b) => b.value - a.value);

  return (
    <div className="grid h-full w-full grid-cols-3 grid-rows-2 gap-1 p-2">
      {ordered.map(({ value, index }, position) => (
        <div
          key={data.categories[index]}
          className="flex flex-col justify-end overflow-hidden rounded p-2 text-white"
          style={{
            backgroundColor: PALETTE[index % PALETTE.length],
            gridColumn: position === 0 ? 'span 2' : undefined,
            gridRow: position === 0 ? 'span 2' : undefined,
          }}
        >
          <span className="truncate text-xs font-medium">{data.categories[index]}</span>
          <span className="text-[10px] opacity-80">{Math.round((value / sum) * 100)}%</span>
        </div>
      ))}
    </div>
  );
};

const HeatmapChart: React.FC<{ data: SampleData }> = ({ data }) => {
  const max = Math.max(...data.values.flat(), 1);
  return (
    <div className="flex h-full w-full flex-col justify-center p-4">
      <div className="flex">
        <div className="w-16" />
        {data.measures.map((measure) => (
          <div key={measure} className="flex-1 truncate px-1 text-center text-[9px] text-gray-500">
            {measure}
          </div>
        ))}
      </div>
      {data.categories.map((category, ci) => (
        <div key={category} className="flex items-center">
          <div className="w-16 truncate pr-1 text-right text-[9px] text-gray-500">{category}</div>
          {data.measures.map((measure, mi) => {
            const intensity = data.values[ci][mi] / max;
            return (
              <div key={measure} className="flex-1 px-0.5 py-0.5">
                <div
                  className="flex h-7 items-center justify-center rounded text-[9px] font-medium"
                  style={{
                    backgroundColor: `rgba(37, 99, 235, ${0.15 + intensity * 0.75})`,
                    color: intensity > 0.55 ? '#fff' : '#1e3a8a',
                  }}
                >
                  {data.values[ci][mi]}
                </div>
              </div>
            );
          })}
        </div>
      ))}
    </div>
  );
};

const TablePreview: React.FC<{ data: SampleData }> = ({ data }) => (
  <div className="h-full w-full overflow-auto p-2">
    <table className="w-full border-collapse text-xs">
      <thead>
        <tr className="border-b border-gray-200 text-left text-gray-500">
          <th className="px-2 py-1.5 font-medium">Category</th>
          {data.measures.map((measure) => (
            <th key={measure} className="px-2 py-1.5 text-right font-medium">
              {measure}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {data.categories.map((category, ci) => (
          <tr key={category} className="border-b border-gray-100">
            <td className="px-2 py-1.5 text-gray-700">{category}</td>
            {data.measures.map((measure, mi) => (
              <td key={measure} className="px-2 py-1.5 text-right font-mono text-gray-600">
                {data.values[ci][mi]}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);

const BigNumber: React.FC<{ data: SampleData }> = ({ data }) => {
  const total = data.values.reduce((acc, row) => acc + row[0], 0);
  return (
    <div className="flex h-full w-full flex-col items-center justify-center">
      <span className="text-5xl font-semibold text-gray-900">{total.toLocaleString()}</span>
      <span className="mt-2 text-sm text-gray-500">{data.measures[0]}</span>
    </div>
  );
};

const ChartCanvas: React.FC<ChartCanvasProps> = ({ vizType, wells }) => {
  const data = useMemo(() => buildSampleData(wells), [wells]);

  const chart = (() => {
    switch (vizType) {
      case 'Line Chart':
      case 'Time-series':
        return <LineChart data={data} />;
      case 'Area Chart':
        return <LineChart data={data} area />;
      case 'Pie Chart':
        return <PieChart data={data} />;
      case 'Treemap':
        return <TreemapChart data={data} />;
      case 'Heatmap':
        return <HeatmapChart data={data} />;
      case 'Table':
        return <TablePreview data={data} />;
      case 'Big Number':
        return <BigNumber data={data} />;
      case 'World Map':
        return (
          <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">
            Map preview is not available yet.
          </div>
        );
      default:
        return <BarChart data={data} />;
    }
  })();

  return (
    <div className="flex h-full w-full flex-col">
      <div className="flex-1 overflow-hidden p-2">{chart}</div>
      <p className="px-2 pb-1 text-center text-[10px] text-gray-400">Preview with sample data</p>
    </div>
  );
};

export default ChartCanvas;
