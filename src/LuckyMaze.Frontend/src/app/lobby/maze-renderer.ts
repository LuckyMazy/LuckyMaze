import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { MazeCell } from '../api/model/mazeCell';
import { MazeExit } from '../api/model/mazeExit';

@Component({
  selector: 'luckymaze-maze-renderer',
  standalone: true,
  template: `
    <div class="relative w-full aspect-square max-w-[450px] mx-auto border border-white/10 rounded-2xl overflow-hidden bg-slate-950/60 backdrop-blur-md shadow-2xl">
      <canvas #mazeCanvas class="w-full h-full block"></canvas>
    </div>
  `
})
export class MazeRenderer implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('mazeCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  @Input() width: number = 7;
  @Input() height: number = 7;
  @Input() gridData: Array<MazeCell> | null = null;
  @Input() exits: Array<MazeExit> | null = null;
  @Input() aiPosition: { x: number; y: number } | null = null;

  private ctx!: CanvasRenderingContext2D;
  private animationFrameId: number | null = null;

  // AI interpolation values
  private currentAiX: number | null = null;
  private currentAiY: number | null = null;
  private targetAiX: number | null = null;
  private targetAiY: number | null = null;
  private lerpSpeed = 0.15; // Speed of AI smooth transition

  ngAfterViewInit(): void {
    const canvas = this.canvasRef.nativeElement;
    this.ctx = canvas.getContext('2d')!;
    this.resizeCanvas();
    this.startRenderLoop();

    // Listen to resize events
    window.addEventListener('resize', this.onResize);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['aiPosition'] && this.aiPosition) {
      if (this.currentAiX === null || this.currentAiY === null) {
        // First position, snap immediately
        this.currentAiX = this.aiPosition.x;
        this.currentAiY = this.aiPosition.y;
      }
      this.targetAiX = this.aiPosition.x;
      this.targetAiY = this.aiPosition.y;
    }
  }

  ngOnDestroy(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
    window.removeEventListener('resize', this.onResize);
  }

  private onResize = () => {
    this.resizeCanvas();
  };

  private resizeCanvas(): void {
    const canvas = this.canvasRef.nativeElement;
    const rect = canvas.getBoundingClientRect();
    // Support high-DPI screens
    const dpr = window.devicePixelRatio || 1;
    canvas.width = rect.width * dpr;
    canvas.height = rect.height * dpr;
    this.ctx.scale(dpr, dpr);
  }

  private startRenderLoop(): void {
    const render = () => {
      this.draw();
      this.animationFrameId = requestAnimationFrame(render);
    };
    render();
  }

  private draw(): void {
    if (!this.ctx || !this.gridData) return;

    const canvas = this.canvasRef.nativeElement;
    const drawWidth = canvas.width / (window.devicePixelRatio || 1);
    const drawHeight = canvas.height / (window.devicePixelRatio || 1);

    // Clear canvas
    this.ctx.clearRect(0, 0, drawWidth, drawHeight);

    // Calculate grid dimensions
    const cellWidth = drawWidth / this.width;
    const cellHeight = drawHeight / this.height;

    // Draw floor grid pattern (subtle squares)
    this.ctx.strokeStyle = 'rgba(255, 255, 255, 0.03)';
    this.ctx.lineWidth = 1;
    for (let x = 0; x <= this.width; x++) {
      this.ctx.beginPath();
      this.ctx.moveTo(x * cellWidth, 0);
      this.ctx.lineTo(x * cellWidth, drawHeight);
      this.ctx.stroke();
    }
    for (let y = 0; y <= this.height; y++) {
      this.ctx.beginPath();
      this.ctx.moveTo(0, y * cellHeight);
      this.ctx.lineTo(drawWidth, y * cellHeight);
      this.ctx.stroke();
    }

    // Draw cells (walls)
    this.ctx.strokeStyle = '#6366f1'; // Indigo wall color
    this.ctx.lineWidth = 4;
    this.ctx.lineCap = 'round';
    this.ctx.shadowBlur = 10;
    this.ctx.shadowColor = 'rgba(99, 102, 241, 0.5)'; // Wall glow

    for (const cell of this.gridData) {
      const x1 = cell.x * cellWidth;
      const y1 = cell.y * cellHeight;
      const x2 = x1 + cellWidth;
      const y2 = y1 + cellHeight;

      if (cell.north) {
        this.ctx.beginPath();
        this.ctx.moveTo(x1, y1);
        this.ctx.lineTo(x2, y1);
        this.ctx.stroke();
      }
      if (cell.east) {
        this.ctx.beginPath();
        this.ctx.moveTo(x2, y1);
        this.ctx.lineTo(x2, y2);
        this.ctx.stroke();
      }
      if (cell.south) {
        this.ctx.beginPath();
        this.ctx.moveTo(x1, y2);
        this.ctx.lineTo(x2, y2);
        this.ctx.stroke();
      }
      if (cell.west) {
        this.ctx.beginPath();
        this.ctx.moveTo(x1, y1);
        this.ctx.lineTo(x1, y2);
        this.ctx.stroke();
      }
    }

    // Reset shadow for text and other elements
    this.ctx.shadowBlur = 0;

    // Draw exits
    if (this.exits) {
      this.ctx.lineWidth = 2;
      for (const exit of this.exits) {
        const xCenter = (exit.x + 0.5) * cellWidth;
        const yCenter = (exit.y + 0.5) * cellHeight;

        // Draw exit portal ring (orange glow)
        this.ctx.strokeStyle = '#f97316'; // Orange
        this.ctx.shadowBlur = 12;
        this.ctx.shadowColor = 'rgba(249, 115, 22, 0.7)';
        this.ctx.beginPath();
        this.ctx.arc(xCenter, yCenter, Math.min(cellWidth, cellHeight) * 0.28, 0, Math.PI * 2);
        this.ctx.stroke();
        this.ctx.shadowBlur = 0;

        // Draw text label: "Exit A", "Exit B"
        this.ctx.fillStyle = '#f97316';
        this.ctx.font = 'bold 12px Outfit, sans-serif';
        this.ctx.textAlign = 'center';
        this.ctx.textBaseline = 'middle';
        this.ctx.fillText(exit.name, xCenter, yCenter);
      }
    }

    // Draw AI Agent (smoothly interpolated)
    if (this.currentAiX !== null && this.currentAiY !== null && this.targetAiX !== null && this.targetAiY !== null) {
      // Linearly interpolate coordinates
      this.currentAiX += (this.targetAiX - this.currentAiX) * this.lerpSpeed;
      this.currentAiY += (this.targetAiY - this.currentAiY) * this.lerpSpeed;

      const aiDrawX = (this.currentAiX + 0.5) * cellWidth;
      const aiDrawY = (this.currentAiY + 0.5) * cellHeight;
      const aiRadius = Math.min(cellWidth, cellHeight) * 0.22;

      // Draw AI neon green orb
      this.ctx.shadowBlur = 18;
      this.ctx.shadowColor = 'rgba(16, 185, 129, 0.9)'; // Emerald glow
      
      const gradient = this.ctx.createRadialGradient(aiDrawX, aiDrawY, aiRadius * 0.2, aiDrawX, aiDrawY, aiRadius);
      gradient.addColorStop(0, '#34d399'); // Lighter green center
      gradient.addColorStop(1, '#10b981'); // Emerald green border

      this.ctx.fillStyle = gradient;
      this.ctx.beginPath();
      this.ctx.arc(aiDrawX, aiDrawY, aiRadius, 0, Math.PI * 2);
      this.ctx.fill();

      // Draw inner glowing core
      this.ctx.shadowBlur = 0;
      this.ctx.fillStyle = '#ffffff';
      this.ctx.beginPath();
      this.ctx.arc(aiDrawX - aiRadius * 0.2, aiDrawY - aiRadius * 0.2, aiRadius * 0.25, 0, Math.PI * 2);
      this.ctx.fill();
    }
  }
}
