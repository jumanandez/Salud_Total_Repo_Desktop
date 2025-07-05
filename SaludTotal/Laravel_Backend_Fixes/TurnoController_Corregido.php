<?php

namespace App\Http\Controllers;

use App\Models\Turno;
use Illuminate\Http\Request;
use App\Http\Requests\StoreTurnoRequest;
use App\Http\Requests\UpdateTurnoRequest;
use Illuminate\Support\Facades\Auth;
use App\Models\HorarioDisponible;
use App\Rules\FechaDisponible;
use Carbon\Carbon;
use App\ListarHorariosDisponibles;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class TurnoController extends Controller
{
    /**
     * Transformar un turno a la estructura JSON requerida
     */
    private function transformTurno($turno)
    {
        try {
            $especialidadInfo = null;
            
            // Verificar si existe el doctor y la especialidad
            if ($turno->doctor) {
                if (method_exists($turno->doctor, 'especialidad') && $turno->doctor->especialidad) {
                    $especialidadInfo = [
                        'especialidad_id' => $turno->doctor->especialidad->especialidad_id ?? null,
                        'nombre' => $turno->doctor->especialidad->nombre ?? 'No especificada'
                    ];
                } else {
                    // Fallback: buscar especialidad por ID si la relación no está definida
                    $especialidadId = $turno->doctor->especialidad ?? null;
                    if ($especialidadId) {
                        $especialidad = DB::table('especialidades')->where('especialidad_id', $especialidadId)->first();
                        if ($especialidad) {
                            $especialidadInfo = [
                                'especialidad_id' => $especialidad->especialidad_id,
                                'nombre' => $especialidad->nombre
                            ];
                        }
                    }
                }
            }
            
            return [
                'id' => $turno->id,
                'paciente_id' => $turno->paciente_id,
                'doctor_id' => $turno->doctor_id,
                'fecha' => $turno->fecha,
                'hora' => $turno->hora,
                'estado' => $turno->estado,
                'paciente' => [
                    'id' => $turno->paciente->id ?? null,
                    'name' => $turno->paciente->name ?? 'Paciente no encontrado',
                    'email' => $turno->paciente->email ?? null,
                ],
                'doctor' => [
                    'doctor_id' => $turno->doctor->doctor_id ?? null,
                    'nombre_apellido' => $turno->doctor->nombre_apellido ?? 'Doctor no encontrado',
                    'especialidad' => $especialidadInfo
                ]
            ];
        } catch (\Exception $e) {
            Log::error('Error en transformTurno: ' . $e->getMessage(), [
                'turno_id' => $turno->id ?? 'unknown',
                'trace' => $e->getTraceAsString()
            ]);
            
            // Devolver estructura básica en caso de error
            return [
                'id' => $turno->id,
                'paciente_id' => $turno->paciente_id,
                'doctor_id' => $turno->doctor_id,
                'fecha' => $turno->fecha,
                'hora' => $turno->hora,
                'estado' => $turno->estado,
                'paciente' => ['id' => null, 'name' => 'Error al cargar', 'email' => null],
                'doctor' => ['doctor_id' => null, 'nombre_apellido' => 'Error al cargar', 'especialidad' => null]
            ];
        }
    }

    /**
     * Show the form for creating a new resource.
     */
    public function create()
    {
        return view('turnos.create');
    }

    public function index()
    {
        try {
            // Intentar cargar con relaciones
            $turnos = Turno::with(['paciente', 'doctor'])->get();
            
            $turnosTransformados = $turnos->map(function ($turno) {
                return $this->transformTurno($turno);
            });
            
            return response()->json($turnosTransformados);
        } catch (\Exception $e) {
            Log::error('Error en index de turnos: ' . $e->getMessage(), [
                'trace' => $e->getTraceAsString()
            ]);
            
            return response()->json([
                'error' => 'Error al obtener turnos',
                'message' => $e->getMessage(),
                'debug_info' => [
                    'line' => $e->getLine(),
                    'file' => basename($e->getFile())
                ]
            ], 500);
        }
    }

    /**
     * Store a newly created resource in storage.
     */
    public function store(StoreTurnoRequest $request)
    {
        try {
            $turnoValidado = $request->validated();
            $turnoNuevo = Turno::create([
                'paciente_id' => Auth::user()->id,
                'doctor_id' => $turnoValidado['doctor_id'],
                'fecha' => $turnoValidado['fecha'],
                'hora' => $turnoValidado['hora'],
                'estado' => 'activo'
            ]);

            // Cargar las relaciones para devolver información completa
            $turnoNuevo->load(['paciente', 'doctor']);

            return response()->json($this->transformTurno($turnoNuevo));
        } catch (\Exception $e) {
            Log::error('Error al crear turno: ' . $e->getMessage());
            return response()->json([
                'error' => 'Error al crear turno',
                'message' => $e->getMessage()
            ], 500);
        }
    }

    public function turnosDisponibles(Request $request)
    {
        try {
            $validated = $request->validate([
                'doctor_id' => 'required|exists:doctores,doctor_id',
                'fecha' => ['required','date_format:Y-m-d','after_or_equal:today', new FechaDisponible($request['doctor_id'])],
            ],
            [
                'fecha.after_or_equal' => 'La fecha debe ser hoy o una fecha futura.',
            ]);

            $diaSemana = Carbon::parse($validated['fecha'], 'America/Argentina/Buenos_Aires')->dayOfWeekIso;

            $infoHorario = HorarioDisponible::where('doctor_id', $validated['doctor_id'])
                ->where('dia_semana', $diaSemana)
                ->first(['hora_inicio','hora_fin']);

            $duracion_slot = DB::table('tiempo_consulta')->where('doctor_id', $validated['doctor_id'])
                ->value('tiempo_minutos');

            $slots = ListarHorariosDisponibles::listarHorariosDisponibles(
                $infoHorario['hora_inicio'],
                $infoHorario['hora_fin'],
                $validated['fecha'],
                $validated['doctor_id'],
                $duracion_slot
            );
            return response()->json($slots);
        } catch (\Exception $e) {
            Log::error('Error en turnosDisponibles: ' . $e->getMessage());
            return response()->json([
                'error' => 'Error al obtener turnos disponibles',
                'message' => $e->getMessage()
            ], 500);
        }
    }

    /**
     * Display the specified resource.
     */
    public function show(Turno $turno)
    {
        //
    }

    /**
     * Show the form for editing the specified resource.
     */
    public function edit(Turno $turno)
    {
        //
    }

    /**
     * Update the specified resource in storage.
     */
    public function update(UpdateTurnoRequest $request, Turno $turno)
    {
        //
    }

    /**
     * Filtrar turnos por especialidad - VERSIÓN MEJORADA
     */
    public function filterByEspecialidad(Request $request)
    {
        try {
            $validated = $request->validate([
                'especialidad_id' => 'required|integer|min:1',
            ]);

            Log::info('Filtrando turnos por especialidad', ['especialidad_id' => $validated['especialidad_id']]);

            // Método 1: Intentar con relación Eloquent
            try {
                $turnos = Turno::with(['paciente', 'doctor.especialidad'])
                    ->whereHas('doctor', function ($query) use ($validated) {
                        $query->where('especialidad', $validated['especialidad_id']);
                    })
                    ->get();
                    
                Log::info('Turnos encontrados con relación Eloquent: ' . $turnos->count());
            } catch (\Exception $e) {
                Log::warning('Falló método con relación Eloquent: ' . $e->getMessage());
                
                // Método 2: Fallback usando JOIN directo
                $turnos = Turno::with(['paciente', 'doctor'])
                    ->join('doctores', 'turnos.doctor_id', '=', 'doctores.doctor_id')
                    ->where('doctores.especialidad', $validated['especialidad_id'])
                    ->select('turnos.*')
                    ->get();
                    
                Log::info('Turnos encontrados con JOIN: ' . $turnos->count());
            }

            $turnosTransformados = $turnos->map(function ($turno) {
                return $this->transformTurno($turno);
            });

            return response()->json($turnosTransformados);
        } catch (\Exception $e) {
            Log::error('Error al filtrar turnos por especialidad: ' . $e->getMessage(), [
                'especialidad_id' => $request->get('especialidad_id'),
                'trace' => $e->getTraceAsString()
            ]);
            
            return response()->json([
                'error' => 'Error al filtrar turnos',
                'message' => $e->getMessage(),
                'debug_info' => [
                    'line' => $e->getLine(),
                    'file' => basename($e->getFile()),
                    'especialidad_id' => $request->get('especialidad_id')
                ]
            ], 500);
        }
    }

    /**
     * Cancelar un turno
     */
    public function cancelar(Request $request, $id)
    {
        try {
            $turno = Turno::findOrFail($id);
            
            // Verificar que el usuario autenticado sea el dueño del turno
            if ($turno->paciente_id !== Auth::user()->id) {
                return response()->json([
                    'error' => 'No tienes permisos para cancelar este turno'
                ], 403);
            }

            // Verificar que el turno no esté ya cancelado
            if ($turno->estado === 'cancelado') {
                return response()->json([
                    'error' => 'Este turno ya está cancelado'
                ], 400);
            }

            $turno->update(['estado' => 'cancelado']);

            // Cargar las relaciones para devolver información completa
            $turno->load(['paciente', 'doctor']);

            return response()->json([
                'message' => 'Turno cancelado exitosamente',
                'turno' => $this->transformTurno($turno)
            ]);
        } catch (\Exception $e) {
            Log::error('Error al cancelar turno: ' . $e->getMessage());
            return response()->json([
                'error' => 'Error al cancelar turno',
                'message' => $e->getMessage()
            ], 500);
        }
    }

    /**
     * Reprogramar un turno
     */
    public function reprogramar(Request $request, $id)
    {
        try {
            $turno = Turno::findOrFail($id);
            
            // Verificar que el usuario autenticado sea el dueño del turno
            if ($turno->paciente_id !== Auth::user()->id) {
                return response()->json([
                    'error' => 'No tienes permisos para reprogramar este turno'
                ], 403);
            }

            // Verificar que el turno esté activo
            if ($turno->estado !== 'activo') {
                return response()->json([
                    'error' => 'Solo se pueden reprogramar turnos activos'
                ], 400);
            }

            $validated = $request->validate([
                'fecha' => ['required', 'date_format:Y-m-d', 'after_or_equal:today', new FechaDisponible($turno->doctor_id)],
                'hora' => 'required|date_format:H:i',
            ]);

            // Verificar que la nueva fecha y hora estén disponibles
            $diaSemana = Carbon::parse($validated['fecha'], 'America/Argentina/Buenos_Aires')->dayOfWeekIso;
            
            $infoHorario = HorarioDisponible::where('doctor_id', $turno->doctor_id)
                ->where('dia_semana', $diaSemana)
                ->first(['hora_inicio', 'hora_fin']);

            if (!$infoHorario) {
                return response()->json([
                    'error' => 'El doctor no tiene horarios disponibles para esta fecha'
                ], 400);
            }

            // Verificar que la hora esté dentro del horario del doctor
            if ($validated['hora'] < $infoHorario->hora_inicio || $validated['hora'] >= $infoHorario->hora_fin) {
                return response()->json([
                    'error' => 'La hora seleccionada está fuera del horario de atención del doctor'
                ], 400);
            }

            // Verificar que no haya otro turno en esa fecha y hora
            $turnoExistente = Turno::where('doctor_id', $turno->doctor_id)
                ->where('fecha', $validated['fecha'])
                ->where('hora', $validated['hora'])
                ->where('estado', 'activo')
                ->where('id', '!=', $turno->id)
                ->first();

            if ($turnoExistente) {
                return response()->json([
                    'error' => 'Ya existe un turno para esa fecha y hora'
                ], 400);
            }

            $turno->update([
                'fecha' => $validated['fecha'],
                'hora' => $validated['hora']
            ]);

            return response()->json([
                'message' => 'Turno reprogramado exitosamente',
                'turno' => $this->transformTurno($turno->load(['paciente', 'doctor']))
            ]);
        } catch (\Exception $e) {
            Log::error('Error al reprogramar turno: ' . $e->getMessage());
            return response()->json([
                'error' => 'Error al reprogramar turno',
                'message' => $e->getMessage()
            ], 500);
        }
    }

    /**
     * Obtener turnos del usuario autenticado with filtros opcionales
     */
    public function misTurnosApi(Request $request)
    {
        try {
            $query = Turno::with(['paciente', 'doctor'])
                ->where('paciente_id', Auth::user()->id);

            // Filtro por estado
            if ($request->has('estado')) {
                $query->where('estado', $request->estado);
            }

            // Filtro por especialidad
            if ($request->has('especialidad_id')) {
                $query->whereHas('doctor', function ($q) use ($request) {
                    $q->where('especialidad', $request->especialidad_id);
                });
            }

            // Filtro por fecha
            if ($request->has('fecha_desde')) {
                $query->where('fecha', '>=', $request->fecha_desde);
            }

            if ($request->has('fecha_hasta')) {
                $query->where('fecha', '<=', $request->fecha_hasta);
            }

            $turnos = $query->orderBy('fecha', 'asc')
                ->orderBy('hora', 'asc')
                ->get();

            $turnosTransformados = $turnos->map(function ($turno) {
                return $this->transformTurno($turno);
            });

            return response()->json($turnosTransformados);
        } catch (\Exception $e) {
            Log::error('Error al obtener mis turnos: ' . $e->getMessage());
            return response()->json([
                'error' => 'Error al obtener mis turnos',
                'message' => $e->getMessage()
            ], 500);
        }
    }

    public function misTurnos()
    {
        try {
            $user = Auth::user();
            $turnos = Turno::where('paciente_id', $user->id)->get();
            return view('turnos.misTurnos', compact('turnos'));
        } catch (\Exception $e) {
            Log::error('Error en misTurnos: ' . $e->getMessage());
            return redirect()->back()->with('error', 'Error al cargar turnos');
        }
    }

    /**
     * Endpoint de prueba para verificar conexiones - MEJORADO
     */
    public function test()
    {
        try {
            $response = [
                'status' => 'success',
                'timestamp' => now()->toISOString(),
                'database_connection' => 'OK',
                'counts' => [],
                'sample_data' => [],
                'table_checks' => [],
                'message' => 'Prueba de conexión exitosa'
            ];

            // Prueba 1: Conexión básica a la tabla turnos
            try {
                $turnosCount = Turno::count();
                $response['counts']['turnos'] = $turnosCount;
                $response['table_checks']['turnos'] = 'OK';
            } catch (\Exception $e) {
                $response['counts']['turnos'] = 0;
                $response['table_checks']['turnos'] = 'ERROR: ' . $e->getMessage();
            }
            
            // Prueba 2: Verificar si existen doctores
            try {
                $doctoresCount = DB::table('doctores')->count();
                $response['counts']['doctores'] = $doctoresCount;
                $response['table_checks']['doctores'] = 'OK';
            } catch (\Exception $e) {
                $response['counts']['doctores'] = 0;
                $response['table_checks']['doctores'] = 'ERROR: ' . $e->getMessage();
            }
            
            // Prueba 3: Verificar si existen especialidades
            try {
                $especialidadesCount = DB::table('especialidades')->count();
                $response['counts']['especialidades'] = $especialidadesCount;
                $response['table_checks']['especialidades'] = 'OK';
                
                // Obtener lista de especialidades
                $especialidades = DB::table('especialidades')->select('especialidad_id', 'nombre')->get();
                $response['sample_data']['especialidades'] = $especialidades;
            } catch (\Exception $e) {
                $response['counts']['especialidades'] = 0;
                $response['table_checks']['especialidades'] = 'ERROR: ' . $e->getMessage();
            }
            
            // Prueba 4: Verificar si existen usuarios
            try {
                $usuariosCount = DB::table('users')->count();
                $response['counts']['usuarios'] = $usuariosCount;
                $response['table_checks']['users'] = 'OK';
            } catch (\Exception $e) {
                $response['counts']['usuarios'] = 0;
                $response['table_checks']['users'] = 'ERROR: ' . $e->getMessage();
            }
            
            // Prueba 5: Intentar obtener un turno con relaciones
            try {
                if ($response['counts']['turnos'] > 0) {
                    $turnoConRelaciones = Turno::with(['paciente', 'doctor'])->first();
                    if ($turnoConRelaciones) {
                        $response['sample_data']['turno'] = $this->transformTurno($turnoConRelaciones);
                    }
                }
            } catch (\Exception $e) {
                $response['sample_data']['turno_error'] = $e->getMessage();
            }

            // Prueba 6: Verificar estructura de tabla doctores
            try {
                $doctorSample = DB::table('doctores')->first();
                $response['sample_data']['doctor_structure'] = $doctorSample;
            } catch (\Exception $e) {
                $response['sample_data']['doctor_error'] = $e->getMessage();
            }
            
            return response()->json($response);
            
        } catch (\Exception $e) {
            Log::error('Error en test: ' . $e->getMessage());
            return response()->json([
                'status' => 'error',
                'message' => $e->getMessage(),
                'line' => $e->getLine(),
                'file' => basename($e->getFile()),
                'timestamp' => now()->toISOString()
            ], 500);
        }
    }
}
